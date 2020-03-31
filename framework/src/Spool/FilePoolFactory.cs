using Microsoft.Extensions.DependencyInjection;

namespace Spool
{
    /// <summary>文件池管理
    /// </summary>
    public class FilePoolFactory : IFilePoolFactory
    {
        private readonly ISpoolHost _host;

        /// <summary>Ctor
        /// </summary>
        public FilePoolFactory(ISpoolHost host)
        {
            _host = host;
        }

        /// <summary>根据文件池描述生成文件池选项
        /// </summary>
        public FilePoolOption BuildOption(FilePoolDescriptor descriptor)
        {
            var option = new FilePoolOption()
            {
                Name = descriptor.Name,
                Path = descriptor.Path,
                EnableFileWatcher = descriptor.EnableFileWatcher,
                FileWatcherPath = descriptor.FileWatcherPath,
                MaxFileWriterCount = descriptor.MaxFileWriterCount,
                WriteBufferSize = descriptor.WriteBufferSize,
                EnableAutoReturn = descriptor.EnableAutoReturn,
                AutoReturnSeconds = descriptor.AutoReturnSeconds,
                ScanReturnFileMillSeconds = descriptor.ScanReturnFileMillSeconds
            };
            return option;
        }

        /// <summary>创建文件池
        /// </summary>
        public FilePool CreateFilePool(FilePoolDescriptor descriptor)
        {
            using (var scope = _host.Provider.CreateScope())
            {
                var scopeOption = scope.ServiceProvider.GetService<FilePoolOption>();
                SetScopeOption(scopeOption, descriptor);
                return scope.ServiceProvider.GetService<FilePool>();
            }
        }


        /// <summary>根据FilePoolDescriptor 设置FilePoolOption的值
        /// </summary>
        public void SetScopeOption(FilePoolOption scopeOption, FilePoolDescriptor descriptor)
        {
            scopeOption.Name = descriptor.Name;
            scopeOption.Path = descriptor.Path;
            scopeOption.EnableFileWatcher = descriptor.EnableFileWatcher;
            scopeOption.FileWatcherPath = descriptor.FileWatcherPath;
            scopeOption.MaxFileWriterCount = descriptor.MaxFileWriterCount;
            scopeOption.WriteBufferSize = descriptor.WriteBufferSize;
            scopeOption.EnableAutoReturn = descriptor.EnableAutoReturn;
            scopeOption.AutoReturnSeconds = descriptor.AutoReturnSeconds;
            scopeOption.ScanReturnFileMillSeconds = descriptor.ScanReturnFileMillSeconds;
        }


        /// <summary>给FilePoolOption赋值
        /// </summary>
        /// <param name="scopeOption">scope生命周期配置信息</param>
        /// <param name="option">当前文件池配置信息</param>
        public void SetScopeOption(FilePoolOption scopeOption, FilePoolOption option)
        {
            scopeOption.Name = option.Name;
            scopeOption.Path = option.Path;
            scopeOption.EnableFileWatcher = option.EnableFileWatcher;
            scopeOption.FileWatcherPath = option.FileWatcherPath;
            scopeOption.MaxFileWriterCount = option.MaxFileWriterCount;
            scopeOption.WriteBufferSize = option.WriteBufferSize;
            scopeOption.EnableAutoReturn = option.EnableAutoReturn;
            scopeOption.AutoReturnSeconds = option.AutoReturnSeconds;
            scopeOption.ScanReturnFileMillSeconds = option.ScanReturnFileMillSeconds;
        }

    }
}
