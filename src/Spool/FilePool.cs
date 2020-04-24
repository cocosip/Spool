using Microsoft.Extensions.Logging;
using Spool.Extensions;
using Spool.Scheduling;
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
        private int _inFileWatcher = 0;
        private int _isRunning = 0;

        private readonly ILogger _logger;
        private readonly IScheduleService _scheduleService;
        private readonly ITrainManager _trainManager;

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
        public FilePool(ILogger<FilePool> logger, IScheduleService scheduleService, ITrainManager trainManager, FilePoolOption option)
        {
            _logger = logger;
            _scheduleService = scheduleService;
            _trainManager = trainManager;
            Option = option;

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
                StartScanTimeoutTakeFileTask();
            }

            //文件夹监控
            if (Option.EnableFileWatcher)
            {
                if (Option.FileWatcherPath.IsNullOrWhiteSpace())
                {
                    throw new ArgumentException("监控目录为空,无法启动监控功能!");
                }
                if (DirectoryHelper.CreateIfNotExists(Option.FileWatcherPath))
                {
                    _logger.LogInformation("创建文件池:'{0}'的监控目录:'{1}'.", Option.Name, Option.FileWatcherPath);
                }

                StartScanFileWatcherTask();
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

            if (Option.EnableAutoReturn)
            {
                StopScanTimeoutTakeFileTask();
            }

            //文件夹监控
            if (Option.EnableFileWatcher)
            {
                StopScanFileWatcherTask();
            }

            Interlocked.Exchange(ref _isRunning, 0);
        }


        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteFileAsync(Stream stream, string fileExt)
        {
            var train = _trainManager.GetWriteTrain();
            return await train.WriteFileAsync(stream, fileExt);
        }

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public SpoolFile WriteFile(Stream stream, string fileExt)
        {
            var train = _trainManager.GetWriteTrain();
            return train.WriteFile(stream, fileExt);
        }

        /// <summary>获取指定数量的文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public SpoolFile[] GetFiles(int count = 1)
        {
            var train = _trainManager.GetReadTrain();
            var spoolFiles = train.GetFiles(count).ToList();
            if (spoolFiles.Count < count)
            {
                var secondTrain = _trainManager.GetReadTrain();
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
                var train = _trainManager.GetTrainByIndex(groupSpoolFile.Key);
                train.ReturnFiles(groupSpoolFile.ToArray());
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
                var train = _trainManager.GetTrainByIndex(groupSpoolFile.Key);
                train.ReleaseFiles(groupSpoolFile.ToArray());
            }

            if (Option.EnableAutoReturn)
            {
                TryRemoveTakeFiles(spoolFiles);
            }
        }

        /// <summary>初始化
        /// </summary>
        private void Initialize()
        {
            //创建目录
            if (DirectoryHelper.CreateIfNotExists(Option.Path))
            {
                _logger.LogInformation("创建文件池:'{0}' 的文件目录:'{1}'.", Option.Name, Option.Path);
            }

            //序列管理器初始化
            _trainManager.Initialize();
        }

        /// <summary>开始查询过期的未归还的文件
        /// </summary>
        private void StartScanTimeoutTakeFileTask()
        {
            _scheduleService.StartTask($"FilePool.{Option.Name}.ScanTimeoutTakeFile", ScanTimeoutTakeFile, 1000, Option.ScanReturnFileMillSeconds);
        }

        /// <summary>停止查询过期的未归还的文件
        /// </summary>
        private void StopScanTimeoutTakeFileTask()
        {
            _scheduleService.StopTask($"FilePool.{Option.Name}.ScanTimeoutTakeFile");
        }

        /// <summary>过期未归还文件处理程序
        /// </summary>
        private void ScanTimeoutTakeFile()
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
                    try
                    {
                        ReturnFiles(spoolFileFuture.File);
                        _logger.LogDebug("归还文件:{0}", spoolFileFuture.File);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "系统自动归还文件出错,文件信息:{0}", spoolFileFuture.File);
                    }
                }
                else
                {
                    _logger.LogWarning("移除过期文件失败,文件Key:{0}", key);
                }
            }
        }

        /// <summary>移除指定取走文件
        /// </summary>
        private void TryRemoveTakeFiles(params SpoolFile[] spoolFiles)
        {
            foreach (var spoolFile in spoolFiles)
            {
                if (!_takeFileDict.TryRemove(spoolFile.GenerateCode(), out SpoolFileFuture _))
                {
                    _logger.LogWarning("移除取走的文件失败,{0}", spoolFile);
                }
            }
        }


        /// <summary>开始监控文件任务
        /// </summary>
        private void StartScanFileWatcherTask()
        {
            _scheduleService.StartTask($"FilePool.{Option.Name}.ScanFileWatcher", ScanFileWatcher, 1000, Option.ScanFileWatcherMillSeconds);
        }

        /// <summary>停止监控文件任务
        /// </summary>
        private void StopScanFileWatcherTask()
        {
            _scheduleService.StopTask($"FilePool.{Option.Name}.ScanFileWatcher");
        }

        /// <summary>查询监控目录
        /// </summary>
        private void ScanFileWatcher()
        {
            if (_inFileWatcher == 1)
            {
                _logger.LogWarning("正在进行监控目录的扫描,不会重复进入.文件池:'{0}',监控路径:{1}.", Option.Name, Option.FileWatcherPath);
                return;
            }
            var deleteFiles = new List<string>();
            try
            {
                Interlocked.Exchange(ref _inFileWatcher, 1);
                var directoryInfo = new DirectoryInfo(Option.FileWatcherPath);
                var files = directoryInfo.GetFiles();
                foreach (var file in files)
                {
                    //最后写入的时间是2秒前
                    if (file.LastAccessTime < DateTime.Now.AddSeconds(-2))
                    {
                        var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                        var fileExt = PathUtil.GetPathExtension(file.Name);
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
                            FileHelper.DeleteIfExists(deleteFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "删除监控文件出错,异常信息:{0}.", ex.Message);
                }

                Interlocked.Exchange(ref _inFileWatcher, 0);
            }

        }



    }
}
