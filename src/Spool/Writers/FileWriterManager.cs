using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;

namespace Spool.Writers
{
    /// <summary>文件写入管理器
    /// </summary>
    public class FileWriterManager : IFileWriterManager
    {
        private int _fileWriterCount = 0;
        private readonly int _maxFileWriterCount = int.MaxValue;
        private readonly ConcurrentStack<FileWriter> _fileWriterStack;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly ILogger _logger;
        private readonly ISpoolHost _host;
        private readonly IFilePoolFactory _filePoolFactory;
        private readonly FilePoolOption _option;


        /// <summary>Ctor
        /// </summary>
        public FileWriterManager(ILogger<FileWriterManager> logger, ISpoolHost host, IFilePoolFactory filePoolFactory, FilePoolOption option)
        {
            _logger = logger;
            _host = host;
            _filePoolFactory = filePoolFactory;
            _option = option;

            if (_option.MaxFileWriterCount > 0)
            {
                _maxFileWriterCount = _option.MaxFileWriterCount;
            }

            _fileWriterStack = new ConcurrentStack<FileWriter>();
            _autoResetEvent = new AutoResetEvent(false);
        }


        /// <summary>获取一个文件写入器
        /// </summary>
        public FileWriter Get()
        {
            if (!_fileWriterStack.TryPop(out FileWriter fileWriter))
            {
                //未在集合中获取,则判断能否新建
                if (_fileWriterCount < _maxFileWriterCount)
                {
                    fileWriter = CreateWriter();
                    Interlocked.Increment(ref _fileWriterCount);
                    return fileWriter;
                }
                _logger.LogDebug("未能获取新的文件写入器,等待中。文件池:'{0}',路径:'{1}'.", _option.Name, _option.Path);
                _autoResetEvent.WaitOne();
                return Get();
            }

            return fileWriter;
        }

        /// <summary>归还一个文件写入器
        /// </summary>
        public void Return(FileWriter fileWriter)
        {
            if (fileWriter != null)
            {
                _fileWriterStack.Push(fileWriter);
            }
            _autoResetEvent.Set();
        }


        /// <summary>创建文件写入器
        /// </summary>
        private FileWriter CreateWriter()
        {
            using (var scope = _host.Provider.CreateScope())
            {
                var fileWriter = scope.ServiceProvider.GetService<FileWriter>();
                var option = scope.ServiceProvider.GetService<FilePoolOption>();

                _filePoolFactory.SetScopeOption(option, _option);

                return fileWriter;
            }
        }
    }
}
