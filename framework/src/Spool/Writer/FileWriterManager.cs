using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Spool.Writer
{
    public class FileWriterManager : IFileWriterManager
    {
        private readonly ConcurrentStack<FileWriter> _fileWriterStack;

        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;
        private readonly FileWriterOption _option;

        /// <summary>Ctor
        /// </summary>
        public FileWriterManager(IServiceProvider provider, ILogger<FileWriterManager> logger, FileWriterOption option)
        {
            _provider = provider;
            _logger = logger;
            _option = option;

            _fileWriterStack = new ConcurrentStack<FileWriter>();

            _semaphoreSlim = new SemaphoreSlim(1, option.MaxFileWriterCount);
        }

        public void Initialize()
        {
            for (int i = 0; i < _option.MaxFileWriterCount; i++)
            {
                var fileWriter = _provider.GetService<FileWriter>();
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
                _logger.LogInformation("Can't find any fileWriter in fileWriterManager,current group is '{0}'", _option.GroupName);
                _semaphoreSlim.Wait(5000);
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
                _semaphoreSlim.Release();
            }
        }
    }
}
