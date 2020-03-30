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
        private readonly ConcurrentStack<FileWriter> _fileWriterStack;

        private readonly AutoResetEvent _autoResetEvent;
        private readonly ISpoolApplication _spoolApplication;
        private readonly ILogger _logger;
        private readonly FileWriterOption _option;

        /// <summary>Ctor
        /// </summary>
        public FileWriterManager(ILogger<FileWriterManager> logger, ISpoolApplication spoolApplication, FilePoolOption option)
        {
            _logger = logger;
            _spoolApplication = spoolApplication;
            //_option = option;

            _fileWriterStack = new ConcurrentStack<FileWriter>();
            _autoResetEvent = new AutoResetEvent(false);
        }

        public void Initialize()
        {
            for (int i = 0; i < _option.MaxFileWriterCount; i++)
            {

                //var option

                var fileWriter = _spoolApplication.Provider.GetService<FileWriter>();
                _fileWriterStack.Push(fileWriter);
            }
        }

        /// <summary>Get option
        /// </summary>
        public FileWriterOption GetFileWriterOption()
        {
            return _option;
        }

        /// <summary>Get a file writer
        /// </summary>
        public FileWriter Get()
        {
            if (!_fileWriterStack.TryPop(out FileWriter fileWriter))
            {
                _logger.LogInformation("Can't find any fileWriter in fileWriterManager,current group is '{0}'", _option.Group.Name);
                _autoResetEvent.WaitOne();
            }
            _logger.LogDebug("Get fileWriter from queue,{0}", fileWriter);
            return fileWriter;
        }

        /// <summary>Return a file writer
        /// </summary>
        public void Return(FileWriter fileWriter)
        {
            if (fileWriter != null)
            {
                _fileWriterStack.Push(fileWriter);
            }
            _autoResetEvent.Set();
        }
    }
}
