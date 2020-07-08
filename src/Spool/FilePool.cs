using Microsoft.Extensions.Logging;
using Spool.Extensions;
using Spool.Trains;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>文件池
    /// </summary>
    public class FilePool : IFilePool
    {
        private readonly CancellationTokenSource _cts;
        private int _isRunning = 0;

        private readonly ILogger _logger;
        private readonly ITrainFactory _trainFactory;

        /// <summary>文件池的配置信息
        /// </summary>
        public FilePoolOption Option { get; private set; }

        /// <summary>是否正在运行
        /// </summary>
        public bool IsRunning { get { return _isRunning == 1; } }

        /// <summary>归还文件事件
        /// </summary>
        public event EventHandler<ReturnFileEventArgs> OnFileReturn;

        /// <summary>被取走的文件
        /// </summary>
        private readonly ConcurrentDictionary<string, SpoolFileFuture> _takeFileDict;

        /// <summary>Ctor
        /// </summary>
        public FilePool(ILogger<FilePool> logger, ITrainFactory trainFactory, FilePoolOption option)
        {
            _logger = logger;
            _trainFactory = trainFactory;
            Option = option;

            _cts = new CancellationTokenSource();
            _takeFileDict = new ConcurrentDictionary<string, SpoolFileFuture>();
        }

        /// <summary>运行文件池
        /// </summary>
        public void Start()
        {
            if (_isRunning == 1)
            {
                _logger.LogInformation("当前FilePool:'{0}',路径:{1},正在运行,请勿重复运行。", Option.Name, Option.Path);
                return;
            }
            //序列启动
            Initialize();

            //文件夹监控
            if (Option.EnableAutoReturn)
            {
                StartScanTimeoutFile();
            }

            //文件夹监控
            if (Option.EnableFileWatcher)
            {
                if (string.IsNullOrWhiteSpace(Option.FileWatcherPath))
                {
                    throw new ArgumentException("监控目录为空,无法启动监控功能!");
                }
                if (FilePathUtil.CreateIfNotExists(Option.FileWatcherPath))
                {
                    _logger.LogInformation("创建文件池:'{0}'的监控目录:'{1}'.", Option.Name, Option.FileWatcherPath);
                }

                StartScanFileWatcher();
            }

            Interlocked.Exchange(ref _isRunning, 1);
        }

        /// <summary>关闭文件池
        /// </summary>
        public void Shutdown()
        {
            if (_isRunning == 0)
            {
                _logger.LogInformation("当前FilePool:'{0}',路径:{1},已停止,请勿重复操作。", Option.Name, Option.Path);
                return;
            }

            _cts.Cancel();

            Interlocked.Exchange(ref _isRunning, 0);
        }

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteFileAsync(Stream stream, string fileExt)
        {
            var train = _trainFactory.GetWriteTrain();
            return await train.WriteFileAsync(stream, fileExt);
        }

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public SpoolFile WriteFile(Stream stream, string fileExt)
        {
            var train = _trainFactory.GetWriteTrain();
            return train.WriteFile(stream, fileExt);
        }

        /// <summary>获取指定数量的文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public SpoolFile[] GetFiles(int count = 1)
        {
            var train = _trainFactory.GetReadTrain();
            var spoolFiles = train.GetFiles(count).ToList();
            if (spoolFiles.Count < count)
            {
                var secondTrain = _trainFactory.GetReadTrain();
                var secondSpoolFiles = secondTrain.GetFiles(count - spoolFiles.Count);
                spoolFiles.AddRange(secondSpoolFiles);
            }

            //是否启动自动归还功能
            if (Option.EnableAutoReturn)
            {
                foreach (var spoolFile in spoolFiles)
                {
                    var key = spoolFile.GenerateCode();
                    if (_takeFileDict.ContainsKey(key))
                    {
                        _logger.LogDebug("当前取走的文件中已经包含了该文件,文件Key:'{0}'.", key);
                    }
                    else
                    {
                        var spoolFileFuture = new SpoolFileFuture(spoolFile, Option.AutoReturnSeconds);
                        if (!_takeFileDict.TryAdd(spoolFile.GenerateCode(), spoolFileFuture))
                        {
                            _logger.LogWarning("添加待归还的文件失败:{0}", spoolFile);
                        }
                    }
                }
            }
            return spoolFiles.ToArray();
        }

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        public void ReturnFiles(params SpoolFile[] spoolFiles)
        {
            var groupSpoolFiles = spoolFiles.GroupBy(x => x.TrainIndex);
            foreach (var groupSpoolFile in groupSpoolFiles)
            {
                var train = _trainFactory.GetTrainByIndex(groupSpoolFile.Key);
                if (train != null)
                {
                    train.ReturnFiles(groupSpoolFile.ToArray());
                }
                else
                {
                    _logger.LogWarning("归还数据时,未找到序列号为:'{0}'的序列,该序列可能已经被释放。", groupSpoolFile.Key);
                }
            }

            //自动归还,需要移除文件
            if (Option.EnableAutoReturn)
            {
                TryRemoveTakeFiles(spoolFiles);
            }

            OnFileReturn?.Invoke(this, new ReturnFileEventArgs()
            {
                FilePoolName = Option.Name,
                SpoolFiles = spoolFiles
            });
        }

        /// <summary>释放文件
        /// </summary>
        public void ReleaseFiles(params SpoolFile[] spoolFiles)
        {
            var groupSpoolFiles = spoolFiles.GroupBy(x => x.TrainIndex);
            foreach (var groupSpoolFile in groupSpoolFiles)
            {
                var train = _trainFactory.GetTrainByIndex(groupSpoolFile.Key);
                if (train != null)
                {
                    train.ReleaseFiles(groupSpoolFile.ToArray());
                }
                else
                {
                    _logger.LogWarning("释放数据时,未找到序列号为:'{0}'的序列,该序列可能已经被释放。", groupSpoolFile.Key);
                }
            }

            if (Option.EnableAutoReturn)
            {
                TryRemoveTakeFiles(spoolFiles);
            }
        }

        /// <summary>获取文件数量
        /// </summary>
        public int GetPendingCount()
        {
            var trains = _trainFactory.GetTrains(x => x.TrainType == TrainType.Read || x.TrainType == TrainType.ReadWrite);
            return trains.Sum(x => x.PendingCount);
        }

        /// <summary>获取取走的数量
        /// </summary>
        public int GetProcessingCount()
        {
            return _takeFileDict.Count;
        }

        #region Private method

        /// <summary>初始化
        /// </summary>
        private void Initialize()
        {
            //创建目录
            if (FilePathUtil.CreateIfNotExists(Option.Path))
            {
                _logger.LogInformation("创建文件池:'{0}' 的文件目录:'{1}'.", Option.Name, Option.Path);
            }

            //序列管理器初始化
            _trainFactory.Initialize();
        }

        /// <summary>扫描过期未归还的文件
        /// </summary>
        private void StartScanTimeoutFile()
        {
            Task.Run(async () =>
            {
                await Task.Delay(2000);

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var timeoutKeyList = new List<string>();
                        foreach (var entry in _takeFileDict)
                        {
                            if (entry.Value.IsTimeout())
                            {
                                timeoutKeyList.Add(entry.Key);
                            }
                        }
                        foreach (var key in timeoutKeyList)
                        {

                            if (_takeFileDict.TryRemove(key, out SpoolFileFuture spoolFileFuture))
                            {
                                ReturnFiles(spoolFileFuture.File);
                                _logger.LogDebug("归还文件:{0}", spoolFileFuture.File);
                            }
                            else
                            {
                                _logger.LogWarning("移除过期文件失败,文件Key:{0}", key);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "扫描过期未归还文件出现异常,异常信息:{0}.", ex.Message);
                    }

                    await Task.Delay(Option.ScanReturnFileMillSeconds);
                }
            });
        }

        /// <summary>移除指定取走文件
        /// </summary>
        private void TryRemoveTakeFiles(params SpoolFile[] spoolFiles)
        {
            foreach (var spoolFile in spoolFiles)
            {
                //从被取走的队列中移除元素的时候可能会失败,因为定时任务可能已经移除该超时的文件
                _takeFileDict.TryRemove(spoolFile.GenerateCode(), out SpoolFileFuture _);
            }
        }


        /// <summary>开始监控文件任务
        /// </summary>
        private void StartScanFileWatcher()
        {
            Task.Run(async () =>
            {
                await Task.Delay(2000);

                while (!_cts.Token.IsCancellationRequested)
                {
                    var deleteFiles = new List<string>();
                    try
                    {
                        var files = FilePathUtil.RecursiveGetFileInfos(Option.FileWatcherPath);
                        foreach (var file in files)
                        {
                            //最后写入的时间是2秒前
                            if (file.LastAccessTime < DateTime.Now.AddSeconds(-2))
                            {
                                var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                                var fileExt = FilePathUtil.GetPathExtension(file.Name);
                                WriteFile(fileStream, fileExt);
                                //添加到删除列表
                                deleteFiles.Add(file.FullName);
                                _logger.LogDebug("监控文件:'{0}'被写入到文件池:'{1}'.", file.FullName, Option.Name);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "监控目录出现异常,异常信息:{0}.", ex.Message);
                        //throw ex;
                    }
                    finally
                    {
                        try
                        {
                            if (deleteFiles.Any())
                            {
                                foreach (var deleteFile in deleteFiles)
                                {
                                    FilePathUtil.DeleteFileIfExists(deleteFile);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "删除监控文件出错,异常信息:{0}.", ex.Message);
                        }
                    }

                    await Task.Delay(Option.ScanFileWatcherMillSeconds);
                }

            });
        }

        #endregion


    }
}
