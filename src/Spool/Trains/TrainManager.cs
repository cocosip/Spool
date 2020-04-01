using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Spool.Trains
{
    /// <summary>序列管理器
    /// </summary>
    public class TrainManager : ITrainManager
    {
        private int _isTrainChange = 0;
        private bool _initialized = false;

        /// <summary>锁
        /// </summary>
        private readonly ManualResetEventSlim _manualResetEventSlim;

        /// <summary>序列集合
        /// </summary>
        private readonly ConcurrentDictionary<int, Train> _trainDict;


        private readonly ILogger _logger;
        private readonly ISpoolHost _host;
        private readonly IFilePoolFactory _filePoolFactory;
        private readonly FilePoolOption _option;

        /// <summary>ctor
        /// </summary>
        public TrainManager(ILogger<TrainManager> logger, ISpoolHost host, IFilePoolFactory filePoolFactory, FilePoolOption option)
        {
            _logger = logger;
            _host = host;
            _filePoolFactory = filePoolFactory;
            _option = option;

            _manualResetEventSlim = new ManualResetEventSlim(false);
            _trainDict = new ConcurrentDictionary<int, Train>();
        }


        /// <summary>初始化
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            var trains = LoadExistTrains();
            foreach (var train in trains)
            {
                train.Initialize();
                BindDeleteEvent(train);
                //添加
                _trainDict.TryAdd(train.Index, train);
            }

            //初始化序列
            InitializeTrains();
            _initialized = true;
        }


        /// <summary>根据序列信息,文件池配置信息获取序列基本信息
        /// </summary>
        public TrainInfo BuildInfo(Train train, FilePoolOption option)
        {
            var info = new TrainInfo()
            {
                FilePoolName = option.Name,
                FilePoolPath = option.Path,
                Index = train.Index,
                Name = train.Name,
                Path = train.Path,
                TrainType = train.TrainType
            };
            return info;
        }

 

        /// <summary>获取可以写的序列
        /// </summary>
        public Train GetWriteTrain()
        {
            if (_isTrainChange == 1)
            {
                _manualResetEventSlim.Wait();
            }
            var writeTrain = _trainDict.Values.FirstOrDefault(x => x.TrainType == TrainType.Write || x.TrainType == TrainType.ReadWrite);
            if (writeTrain == null)
            {
                //无写入的Train
                try
                {
                    Interlocked.Exchange(ref _isTrainChange, 1);
                    writeTrain = CreateTrain(GetLatestNextIndex());
                    writeTrain.Initialize();
                    writeTrain.ChangeType(TrainType.Write);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "创建新的写入序列出错!{0}", ex.Message);
                }
                finally
                {
                    Interlocked.Exchange(ref _isTrainChange, 0);
                    _manualResetEventSlim.Set();
                }
            }
            return writeTrain;
        }

        /// <summary>获取可读的序列
        /// </summary>
        public Train GetReadTrain()
        {
            if (_isTrainChange == 1)
            {
                _manualResetEventSlim.Wait();
            }
            var readTrain = _trainDict.Values.FirstOrDefault(x => x.TrainType == TrainType.Write || x.TrainType == TrainType.ReadWrite && !x.IsEmpty());

            if (readTrain == null)
            {
                //获取下一个
                readTrain = _trainDict.Values.OrderBy(x => x.Index).FirstOrDefault(x => x.TrainType == TrainType.Default);
                readTrain.Initialize();
                BindDeleteEvent(readTrain);
            }
            return readTrain;
        }

        /// <summary>创建序列
        /// </summary>
        private Train CreateTrain(int index)
        {
            using (var scope = _host.Provider.CreateScope())
            {
                var trainOption = scope.ServiceProvider.GetService<TrainOption>();
                trainOption.Index = index;
                var filePoolOption = scope.ServiceProvider.GetService<FilePoolOption>();
                _filePoolFactory.SetScopeOption(filePoolOption, _option);
                var train = scope.ServiceProvider.GetService<Train>();
                _logger.LogDebug("创建文件池:'{0}'下面的序列,序列号为:'{1}',", _option.Name, index);

                train.Initialize();
                BindDeleteEvent(train);

                return train;
            }
        }


        /// <summary>加载已经存在的序列
        /// </summary>
        private List<Train> LoadExistTrains()
        {
            var trains = new List<Train>();
            var directoryInfo = new DirectoryInfo(_option.Path);
            var subDirs = directoryInfo.GetDirectories();
            foreach (var subDir in subDirs)
            {
                //是否为序列的文件夹名
                if (TrainUtil.IsTrainName(subDir.Name))
                {
                    //序列索引
                    var index = TrainUtil.GetTrainIndex(subDir.Name);
                    var train = CreateTrain(index);
                    trains.Add(train);
                }
            }
            return trains;
        }


        /// <summary>初始化序列
        /// </summary>
        private void InitializeTrains()
        {
            try
            {
                Interlocked.Exchange(ref _isTrainChange, 1);
                if (_trainDict.Count == 0)
                {
                    var train = CreateTrain(1);
                    train.Initialize();
                    if (!_trainDict.TryAdd(train.Index, train))
                    {
                        throw new ArgumentException($"创建序列失败,文件池名称:{_option.Name},序列:'1' .");
                    }
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
            finally
            {
                Interlocked.Exchange(ref _isTrainChange, 0);
                _manualResetEventSlim.Set();
            }

        }

        /// <summary>获取下一个索引
        /// </summary>
        private int GetLatestNextIndex()
        {
            var latestIndex = _trainDict.Keys.OrderByDescending(x => x).FirstOrDefault();
            return latestIndex++;
        }

        /// <summary>绑定删除事件
        /// </summary>
        private void BindDeleteEvent(Train train)
        {
            train.OnDelete += (s, e) =>
            {
                if (_isTrainChange == 1)
                {
                    _manualResetEventSlim.Wait();
                }

                try
                {
                    Interlocked.Exchange(ref _isTrainChange, 1);
                    //从集合中删除
                    if (!_trainDict.TryRemove(e.Info.Index, out _))
                    {
                        _logger.LogInformation("删除序列失败,文件池:'{0}',索引:'{1}'.", _option.Name, e.Info.Index);
                    }

                }
                finally
                {
                    Interlocked.Exchange(ref _isTrainChange, 0);
                    _manualResetEventSlim.Set();
                }
            };
        }

    }
}
