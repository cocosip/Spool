using Microsoft.Extensions.Logging;
using Spool.Utility;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spool.Worker
{
    /// <summary>
    /// Worker 调度器
    /// </summary>
    public class DefaultWorkerDispatcher : IWorkerDispatcher
    {
        private readonly ILogger _logger;
        private readonly IFileWorkerFactory _fileWorkerFactory;
        private readonly FilePoolConfiguration _configuration;
        private readonly ConcurrentDictionary<int, IFileWorker> _fileWorkerDict;

        /// <summary>
        /// 文件池名称
        /// </summary>
        public string Name => _configuration.Name;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fileWorkerFactory"></param>
        /// <param name="configuration"></param>
        public DefaultWorkerDispatcher(
            ILogger<DefaultWorkerDispatcher> logger,
            IFileWorkerFactory fileWorkerFactory,
            FilePoolConfiguration configuration)
        {
            _logger = logger;
            _fileWorkerFactory = fileWorkerFactory;
            _configuration = configuration;

            _fileWorkerDict = new ConcurrentDictionary<int, IFileWorker>();
        }


        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            //从本地的目录中加载出FileWorkers
            var fileWorkers = _fileWorkerFactory.LoadFileWorkers(_configuration);
            var fileWorkerIndexs = fileWorkers.Select(x => x.Index).ToList();
            _logger.LogInformation("加载WorkerDispatcher '{0}' 下的FileWorkers:{1}.", _configuration.Name, string.Join(",", fileWorkerIndexs));

            if (!fileWorkers.Any())
            {
                var newFileWorker = _fileWorkerFactory.CreateFileWorker(_configuration, 1);
                fileWorkers.Add(newFileWorker);
            }

            //接下去需要初始化FileWorker的状态
            var first = fileWorkers.FirstOrDefault();
            if (fileWorkers.Count == 1)
            {
                //设置为可读可写
            }
            else
            {

            }

        }





    }
}
