using Microsoft.Extensions.Logging;
using Spool.Scheduling;
using Spool.Trains;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>文件池
    /// </summary>
    public class FilePool
    {

        private readonly ILogger _logger;
        private readonly ISpoolHost _host;
        private readonly IScheduleService _scheduleService;
        private readonly ITrainManager _trainManager;

        /// <summary>文件池的配置信息
        /// </summary>
        public FilePoolOption Option { get; private set; }

        /// <summary>是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>被取走的文件
        /// </summary>
        private readonly ConcurrentDictionary<string, SpoolFileFuture> _takeFileDict;


        public FilePool(ILogger<FilePool> logger, ISpoolHost host, IScheduleService scheduleService, ITrainManager trainManager, FilePoolOption option)
        {
            _logger = logger;
            _host = host;
            _scheduleService = scheduleService;
            _trainManager = trainManager;
            Option = option;

            _takeFileDict = new ConcurrentDictionary<string, SpoolFileFuture>();
        }

        /// <summary>运行文件池
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                _logger.LogInformation("当前FilePool:'{0}',路径:{1},正在运行,请勿重复运行。", Option.Name, Option.Path);
                return;
            }
            //序列启动
            Initialize();

            if (Option.EnableAutoReturn)
            {
                StartScanTimeoutTakeFileTask();
            }

            IsRunning = true;
        }


        /// <summary>关闭文件池
        /// </summary>
        public void Shutdown()
        {
            if (!IsRunning)
            {
                _logger.LogInformation("当前FilePool:'{0}',路径:{1},已停止,请勿重复操作。", Option.Name, Option.Path);
                return;
            }

            if (Option.EnableAutoReturn)
            {
                StopScanTimeoutTakeFileTask();
            }

            IsRunning = false;
        }


        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteFile(Stream stream, string fileExt)
        {
            var train = _trainManager.GetWriteTrain();
            return await train.WriteFile(stream, fileExt);
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
                    var spoolFileFuture = new SpoolFileFuture(spoolFile, Option.AutoReturnSeconds);
                    if (!_takeFileDict.TryAdd(spoolFile.GenerateCode(), spoolFileFuture))
                    {
                        _logger.LogWarning("添加待归还的文件失败:{0}", spoolFile);
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


    }
}
