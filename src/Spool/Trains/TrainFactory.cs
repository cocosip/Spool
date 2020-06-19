using Microsoft.Extensions.Logging;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Spool.Trains
{
    /// <summary>序列管理器
    /// </summary>
    public class TrainFactory : ITrainFactory
    {
        private bool _initialized = false;

        /// <summary>锁
        /// </summary>
        private readonly ManualResetEventSlim _manualResetEventSlim;

        /// <summary>序列集合
        /// </summary>
        private readonly ConcurrentDictionary<int, ITrain> _trainDict;


        private readonly ILogger _logger;
        private readonly FilePoolOption _option;
        private readonly ITrainBuilder _trainBuilder;

        /// <summary>ctor
        /// </summary>
        public TrainFactory(ILogger<TrainFactory> logger, FilePoolOption option, ITrainBuilder trainBuilder)
        {
            _logger = logger;
            _option = option;
            _trainBuilder = trainBuilder;

            _manualResetEventSlim = new ManualResetEventSlim(true);
            _trainDict = new ConcurrentDictionary<int, ITrain>();
        }


        /// <summary>初始化
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                _logger.LogInformation("'TrainFactory'已经初始化,请不要重复初始化!");
                return;
            }

            //设置为不可读
            _manualResetEventSlim.Reset();

            var trains = _trainBuilder.BuildPoolTrains(_option);
            foreach (var train in trains)
            {
                //添加
                _trainDict.TryAdd(train.Index, train);
            }

            //初始化序列
            InitializeTrains();
            _initialized = true;
            _manualResetEventSlim.Set();
        }


        /// <summary>根据序列信息,文件池配置信息获取序列基本信息
        /// </summary>
        public TrainInfo BuildInfo(ITrain train)
        {
            var info = new TrainInfo()
            {
                FilePoolName = _option.Name,
                FilePoolPath = _option.Path,
                Index = train.Index,
                Name = train.Name,
                Path = train.Path,
                TrainType = train.TrainType
            };
            return info;
        }

        /// <summary>获取可以写的序列
        /// </summary>
        public ITrain GetWriteTrain()
        {
            _manualResetEventSlim.Wait();

            var writeTrain = _trainDict.Values.FirstOrDefault(x => x.TrainType == TrainType.Write || x.TrainType == TrainType.ReadWrite);
            if (writeTrain == null)
            {
                //无写入的Train
                try
                {
                    _manualResetEventSlim.Reset();
                    writeTrain = _trainBuilder.BuildTrain(GetLatestNextIndex(), _option);

                    //绑定事件
                    BindDefaultEvent(writeTrain);
                    writeTrain.Initialize();

                    writeTrain.ChangeType(TrainType.Write);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "创建新的写入序列出错!{0}", ex.Message);
                    throw ex;
                }
                finally
                {
                    _manualResetEventSlim.Set();
                }
            }
            return writeTrain;
        }

        /// <summary>获取可读的序列
        /// </summary>
        public ITrain GetReadTrain()
        {
            _manualResetEventSlim.Wait();

            //先获取可读的序列
            var readTrain = _trainDict.Values.FirstOrDefault(x => x.TrainType == TrainType.Read && !x.IsEmpty());
            if (readTrain == null)
            {
                //获取一个默认的序列
                readTrain = _trainDict.Values.OrderBy(x => x.Index).FirstOrDefault(x => x.TrainType == TrainType.Default);
                //如果获取的默认序列不为null,就加载数据
                if (readTrain != null)
                {
                    _manualResetEventSlim.Wait();

                    try
                    {
                        //阻塞其他的线程
                        _manualResetEventSlim.Reset();
                        //状态变成可读
                        readTrain.ChangeType(TrainType.Read);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "将'Default'类型序列转换成'Read'类型序列出错了,异常信息:{0}.", ex.Message);
                    }
                    finally
                    {
                        _manualResetEventSlim.Set();
                    }
                }
            }

            //获取可读可写的
            if (readTrain == null)
            {
                readTrain = _trainDict.Values.FirstOrDefault(x => x.TrainType == TrainType.ReadWrite);
            }

            if (readTrain == null)
            {
                _manualResetEventSlim.Wait();

                //获取下一个状态为写的
                var writeTrain = _trainDict.Values.OrderByDescending(x => x.Index).FirstOrDefault(x => x.TrainType == TrainType.Write);

                if (writeTrain != null)
                {
                    try
                    {
                        //阻塞其他的线程
                        _manualResetEventSlim.Reset();
                        writeTrain.ChangeType(TrainType.ReadWrite);
                        return writeTrain;
                    }
                    finally
                    {
                        _manualResetEventSlim.Set();
                    }
                }
            }

            if (readTrain == null)
            {
                throw new ArgumentException("无法获取任何的可读序列!");
            }
            return readTrain;
        }

        /// <summary>根据索引号获取序列
        /// </summary>
        public ITrain GetTrainByIndex(int index)
        {
            _manualResetEventSlim.Wait();
            if (!_trainDict.TryGetValue(index, out ITrain train))
            {
                _logger.LogWarning("获取序列为'{0}'的序列失败,该序列不存在或者已经被释放。", index);
            }
            return train;
        }

        /// <summary>获取全部的序列
        /// </summary>
        public List<ITrain> GetTrains(Func<ITrain, bool> predicate)
        {
            return _trainDict.Values.Where(predicate).ToList();
        }

        #region Private method

        /// <summary>初始化序列
        /// </summary>
        private void InitializeTrains()
        {
            try
            {

                if (_trainDict.Count == 0)
                {
                    var train = _trainBuilder.BuildTrain(1, _option);
                    if (!_trainDict.TryAdd(train.Index, train))
                    {
                        throw new ArgumentException($"创建序列失败,文件池名称:{_option.Name},序列:'1' .");
                    }
                }

                //绑定事件
                foreach (var train in _trainDict.Values)
                {
                    BindDefaultEvent(train);
                    train.Initialize();
                }

                //读写为同一个序列
                if (_trainDict.Count == 1)
                {
                    var train = _trainDict.Values.FirstOrDefault();
                    train.ChangeType(TrainType.ReadWrite);
                }

                //原先有多个
                if (_trainDict.Count >= 2)
                {
                    var latest = _trainDict.Values.OrderByDescending(x => x.Index).FirstOrDefault();
                    latest.ChangeType(TrainType.Write);

                    var first = _trainDict.Values.OrderBy(x => x.Index).FirstOrDefault();
                    first.ChangeType(TrainType.Read);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置序列读写状态出错:{0}.", ex.Message);
                throw ex;
            }
        }

        /// <summary>获取下一个索引
        /// </summary>
        private int GetLatestNextIndex()
        {
            var latestIndex = _trainDict.Keys.OrderByDescending(x => x).FirstOrDefault();
            return latestIndex + 1;
        }

        /// <summary>绑定默认事件
        /// </summary>
        private void BindDefaultEvent(ITrain train)
        {
            train.OnDelete += Train_OnDelete;
            train.OnWriteOver += Train_OnWriteOver;
        }


        private void UnBindDefaultEvent(ITrain train)
        {
            train.OnDelete -= Train_OnDelete;
            train.OnWriteOver -= Train_OnWriteOver;
        }



        private void Train_OnWriteOver(object sender, TrainWriteOverEventArgs e)
        {
            _manualResetEventSlim.Wait();
            try
            {
                _manualResetEventSlim.Reset();
                if (_trainDict.TryGetValue(e.Info.Index, out ITrain train))
                {
                    //如果是可读可写,就变成只读
                    if (train.TrainType == TrainType.ReadWrite || train.TrainType == TrainType.Write)
                    {
                        train.ChangeType(TrainType.Read);
                    }
                    else
                    {
                        _logger.LogInformation("当前序列变成只读,但是原先的类型不为'ReadWrite'或者不为'Write'.");
                    }
                    //创建新的写序列
                    var nextIndex = GetLatestNextIndex();
                    var newWriteTrain = _trainBuilder.BuildTrain(nextIndex, _option);
                    _trainDict.TryAdd(newWriteTrain.Index, newWriteTrain);

                    //绑定事件
                    BindDefaultEvent(newWriteTrain);
                    newWriteTrain.Initialize();

                    //设置为写
                    newWriteTrain.ChangeType(TrainType.Write);
                }
                else
                {
                    _logger.LogInformation("序列写满时,未找到该序列!");
                }

            }
            finally
            {
                _manualResetEventSlim.Set();
            }

        }

        private void Train_OnDelete(object sender, TrainDeleteEventArgs e)
        {
            _manualResetEventSlim.Wait();
            try
            {
                _manualResetEventSlim.Reset();
                //从集合中删除
                if (_trainDict.TryRemove(e.Info.Index, out ITrain train))
                {
                    //删除序列文件
                    FilePathUtil.DeleteDirIfExist(e.Info.Path);
                }
                else
                {
                    _logger.LogWarning("删除序列失败,文件池:'{0}',索引:'{1}'.", _option.Name, e.Info.Index);
                }
                //解绑事件,避免内存泄露
                if (train != null)
                {
                    UnBindDefaultEvent(train);
                }

            }
            finally
            {
                _manualResetEventSlim.Set();
            }
        }
        #endregion
    }
}
