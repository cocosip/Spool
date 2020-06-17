using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Spool.Trains
{
    /// <summary>序列创建器
    /// </summary>
    public class TrainBuilder : ITrainBuilder
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>Ctor
        /// </summary>
        public TrainBuilder(ILogger<TrainBuilder> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>创建序列
        /// </summary>
        /// <param name="option">序列配置信息</param>
        /// <param name="filePoolOption">文件池配置信息</param>
        /// <returns></returns>
        public ITrain BuildTrain(TrainOption option, FilePoolOption filePoolOption)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var injectOption = scope.ServiceProvider.GetService<TrainOption>();
                injectOption.Index = option.Index;

                var injectFilePoolOption = scope.ServiceProvider.GetService<FilePoolOption>();
                injectFilePoolOption.Name = filePoolOption.Name;
                injectFilePoolOption.Path = filePoolOption.Path;

                injectFilePoolOption.MaxFileWriterCount = filePoolOption.MaxFileWriterCount;
                injectFilePoolOption.ConcurrentFileWriterCount = filePoolOption.ConcurrentFileWriterCount;
                injectFilePoolOption.WriteBufferSize = filePoolOption.WriteBufferSize;
                
                injectFilePoolOption.TrainMaxFileCount = filePoolOption.TrainMaxFileCount;
                injectFilePoolOption.EnableFileWatcher = filePoolOption.EnableFileWatcher;
                injectFilePoolOption.FileWatcherPath = filePoolOption.FileWatcherPath;
                injectFilePoolOption.ScanFileWatcherMillSeconds = filePoolOption.ScanFileWatcherMillSeconds;
                injectFilePoolOption.EnableAutoReturn = filePoolOption.EnableAutoReturn;
                injectFilePoolOption.ScanReturnFileMillSeconds = filePoolOption.ScanReturnFileMillSeconds;
                injectFilePoolOption.AutoReturnSeconds = filePoolOption.AutoReturnSeconds;

                var train = scope.ServiceProvider.GetService<ITrain>();
                _logger.LogDebug("创建文件池:'{0}'下面的序列,序列号为:'{1}',", filePoolOption.Name, option.Index);
                return train;
            }
        }




    }
}
