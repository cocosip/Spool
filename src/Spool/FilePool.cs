using Microsoft.Extensions.Logging;
using Spool.Trains;
using Spool.Utility;
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
        private readonly ITrainManager _trainManager;

        /// <summary>文件池的配置信息
        /// </summary>
        public FilePoolOption Option { get; private set; }

        /// <summary>是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>被释放,但是还未处理的文件
        /// </summary>
        private readonly ConcurrentQueue<SpoolFile> _returnFileQueue;


        public FilePool(ILogger<FilePool> logger, ISpoolHost host, ITrainManager trainManager, FilePoolOption option)
        {
            _logger = logger;
            _host = host;
            _trainManager = trainManager;
            Option = option;

            _returnFileQueue = new ConcurrentQueue<SpoolFile>();
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
        public List<SpoolFile> GetFiles(int count = 1)
        {
            var train = _trainManager.GetReadTrain();
            return train.GetFiles(count);
        }

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        public void ReturnFiles(List<SpoolFile> spoolFiles)
        {
            var train = _trainManager.GetReadTrain();
            train.ReturnFiles(spoolFiles);
        }

        /// <summary>释放文件
        /// </summary>
        public void ReleaseFiles(List<SpoolFile> spoolFiles)
        {
            var train = _trainManager.GetReadTrain();
            train.ReleaseFiles(spoolFiles);
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




    }
}
