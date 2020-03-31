using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spool.Dependency;
using Spool.Trains;
using Spool.Utility;
using Spool.Writers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>文件池
    /// </summary>
    public class FilePool
    {
        private readonly ILogger _logger;
        private readonly ISpoolHost _host;
        private readonly IFileWriterManager _fileWriterManager;
        private readonly ITrainFactory _trainFactory;

        /// <summary>文件池的配置信息
        /// </summary>
        public FilePoolOption Option { get; private set; }

        /// <summary>是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>序列集合
        /// </summary>
        private readonly ConcurrentDictionary<int, Train> _trainDict;

        public FilePool(ILogger<FilePool> logger, ISpoolHost host, IFileWriterManager fileWriterManager, ITrainFactory trainFactory, FilePoolOption option)
        {
            _logger = logger;
            _host = host;
            _fileWriterManager = fileWriterManager;
            _trainFactory = trainFactory;
            Option = option;

            _trainDict = new ConcurrentDictionary<int, Train>();
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
            IsRunning = false;
        }



        ///// <summary>创建序列
        ///// </summary>
        //private Train CreateTrain(int index)
        //{
        //    _logger.LogDebug("创建序列,组信息:{0}", _option.Name);
        //    return _spoolApplication.Provider.CreateInstance<Train>(_fileWriterManager, _group, index);
        //}


        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteFile(Stream stream, string fileExt)
        {
            return default;
        }

        /// <summary>获取指定数量的文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public List<SpoolFile> GetFiles(int count = 1)
        {
            return default;
        }

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        public void ReturnFiles(List<SpoolFile> spoolFiles)
        {

        }

        /// <summary>释放文件
        /// </summary>
        public void ReleaseFiles(List<SpoolFile> spoolFiles)
        {

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
            //加载序列
            var trains = _trainFactory.GetTrainsFromPath(Option);


        }
    }
}
