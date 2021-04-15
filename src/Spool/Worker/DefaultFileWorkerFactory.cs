using Microsoft.Extensions.Logging;
using Spool.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spool.Worker
{
    /// <summary>
    /// FileWorker工厂
    /// </summary>
    public class DefaultFileWorkerFactory : IFileWorkerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="loggerFactory"></param>
        public DefaultFileWorkerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }


        /// <summary>
        /// 从文件池的配置文件中,加载本地FileWorkers
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public List<IFileWorker> LoadFileWorkers(FilePoolConfiguration configuration)
        {
            var fileWorkers = new List<IFileWorker>();

            var directory = new DirectoryInfo(configuration.Path);
            var workerDirs = directory.GetDirectories();
            foreach (var workerDir in workerDirs)
            {
                //判断是否为Worker的目录
                if (FileWorkerUtil.IsFileWorkerName(workerDir.Name))
                {
                    var index = FileWorkerUtil.GetIndex(workerDir.Name);
                    var fileWorker = CreateFileWorker(configuration, index);
                    fileWorkers.Add(fileWorker);
                }
            }

            fileWorkers = fileWorkers.OrderBy(x => x.Index).ToList();
            return fileWorkers;
        }

        /// <summary>
        /// 根据参数创建FileWorker
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public IFileWorker CreateFileWorker(FilePoolConfiguration configuration, int index)
        {
            var logger = _loggerFactory.CreateLogger<DefaultFileWorker>();
            var fileWorker = new DefaultFileWorker(logger, configuration, index);
            return fileWorker;
        }

    }
}
