using Microsoft.Extensions.Logging;
using Spool.Trains;
using Spool.Utility;
using Spool.Writers;
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


        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteFile(Stream stream, string fileExt)
        {
            var train = GetWriteTrain();
            return await train.WriteFile(stream, fileExt);
        }

        /// <summary>获取指定数量的文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public List<SpoolFile> GetFiles(int count = 1)
        {
            var train = GetReadTrain();
            return train.GetFiles(count);
        }

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        public void ReturnFiles(List<SpoolFile> spoolFiles)
        {
            var train = GetReadTrain();
            train.ReturnFiles(spoolFiles);
        }

        /// <summary>释放文件
        /// </summary>
        public void ReleaseFiles(List<SpoolFile> spoolFiles)
        {
            var train = GetReadTrain();
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
            //加载序列
            var trains = _trainFactory.GetTrainsFromPath(Option);
            //如果当前目录下不存在任何的序列文件夹,则说明没有序列
            if (!trains.Any())
            {
                //创建一个新的序列(创建第一个序列)
                var train = _trainFactory.CreateTrain(Option, 1);
                trains.Add(train);
            }

            //最新的序列
            var latestTrain = _trainFactory.GetLatest(trains);
            //最新序列变成写,不可读
            latestTrain.ChangeType(TrainType.Write);

            //初始化全部序列
            foreach (var train in trains)
            {
                //train.Initialize();
            }
        }

        /// <summary>获取可写的序列
        /// </summary>
        private Train GetWriteTrain()
        {
            return default;
        }

        /// <summary>获取可读的序列
        /// </summary>
        private Train GetReadTrain()
        {
            return default;
        }

    }
}
