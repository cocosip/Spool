using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spool.Writers
{
    /// <summary>FileWriter创建器
    /// </summary>
    public class FileWriterBuilder : IFileWriterBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>Ctor
        /// </summary>
        public FileWriterBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        /// <summary>创建文件写入器
        /// </summary>
        public IFileWriter BuildWriter(FilePoolOption option)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var injectOption = scope.ServiceProvider.GetService<FileWriterOption>();

                injectOption.Name = option.Name;
                injectOption.Path = option.Path;
                injectOption.MaxFileWriterCount = option.MaxFileWriterCount;
                injectOption.ConcurrentFileWriterCount = option.ConcurrentFileWriterCount;
                injectOption.WriteBufferSize = option.WriteBufferSize;

                var fileWriter = scope.ServiceProvider.GetService<IFileWriter>();
                return fileWriter;
            }
        }
    }
}
