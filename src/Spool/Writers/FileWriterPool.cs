using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;

namespace Spool.Writers
{
    /// <summary>文件写入器池
    /// </summary>
    public class FileWriterPool : IFileWriterPool
    {
        private int _fileWriterCount = 0;
        private readonly int _maxFileWriterCount = int.MaxValue;
        private readonly ConcurrentStack<IFileWriter> _fileWriterStack;
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly ILogger _logger;
        private readonly FilePoolOption _option;
        private readonly IFileWriterBuilder _fileWriterBuilder;

        /// <summary>Ctor
        /// </summary>
        public FileWriterPool(ILogger<FileWriterPool> logger, FilePoolOption option, IFileWriterBuilder fileWriterBuilder)
        {
            _logger = logger;
            _option = option;
            _fileWriterBuilder = fileWriterBuilder;

            if (_option.MaxFileWriterCount > 0)
            {
                _maxFileWriterCount = _option.MaxFileWriterCount;
            }

            _fileWriterStack = new ConcurrentStack<IFileWriter>();
            _semaphoreSlim = new SemaphoreSlim(_option.ConcurrentFileWriterCount);
        }

        /// <summary>获取一个文件写入器
        /// </summary>
        public IFileWriter Get()
        {
            if (!_fileWriterStack.TryPop(out IFileWriter fileWriter))
            {
                //未在集合中获取,则判断能否新建
                if (_fileWriterCount < _maxFileWriterCount)
                {
                    fileWriter = _fileWriterBuilder.BuildWriter(_option);
                    Interlocked.Increment(ref _fileWriterCount);
                    return fileWriter;
                }
                _logger.LogDebug("未能获取新的文件写入器,等待中。文件池:'{0}',路径:'{1}'.", _option.Name, _option.Path);
                _semaphoreSlim.Wait(5000);
                return Get();
            }

            return fileWriter;
        }
        /// <summary>归还一个文件写入器
        /// </summary>
        public void Return(IFileWriter fileWriter)

        {
            if (fileWriter != null)
            {
                _fileWriterStack.Push(fileWriter);
            }
            _semaphoreSlim.Release();
        }



    }
}
