using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spool.IO;

namespace Spool.Workers
{
    public class FileWorker : IWorker
    {

        private int _writtingCount = 0;
        private int _workerMaxFile = 2000;
        private FilePoolConfiguration _configuration;

        private readonly ConcurrentQueue<SpoolFile> _pendingQueue;
        private readonly ConcurrentDictionary<string, SpoolFile> _progressingDict;

        public bool IsSetup { get; private set; } = false;
        public string Name { get; private set; }
        public int Number { get; private set; }
        public string Path { get; private set; }

        private readonly ILogger _logger;
        public FileWorker(
            ILogger<FileWorker> logger,
            FilePoolConfiguration configuration,
            int number)
        {
            _logger = logger;
            _configuration = configuration;
            Number = number;
            Name = WorkerUtil.NormalizeName(Number);
            Path = WorkerUtil.GetPath(_configuration.Path, Name);

            _workerMaxFile = _configuration.WorkerMaxFile;

            _pendingQueue = new ConcurrentQueue<SpoolFile>();
            _progressingDict = new ConcurrentDictionary<string, SpoolFile>();
        }


        public void Setup()
        {
            if (IsSetup)
            {
                _logger.LogInformation("Worker {Name}({Name}) has setup, don't setup repeat!", _configuration.Name, Name);
                return;
            }

            if (DirectoryHelper.CreateIfNotExists(Path))
            {
                _logger.LogInformation("Worker {Name}(Name) create dir {Path}.", _configuration.Name, Name, Path);
            }
        }

        public bool IsPendingEmpty()
        {
            return _pendingQueue.IsEmpty;
        }

        public int GetPendingCount()
        {
            return _pendingQueue.Count;
        }

        // public bool TryEntryWrite()
        // {
        //     Interlocked.Exchange()



        // }

        public async Task<SpoolFile> WriteAsync(Stream stream, string fileExt)
        {

            Interlocked.Increment(ref _writtingCount);

            var spoolFile = new SpoolFile(_configuration.Name, Number);
            try
            {
                return default;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker {Name}({Name}) write failed, {Message}.", _configuration.Name, Name, ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref _writtingCount);
            }

            return default;
        }



        private bool IsFillup()
        {
            if (_writtingCount + _pendingQueue.Count + _progressingDict.Count >= _configuration.WorkerMaxFile)
            {
                return true;
            }
            return false;
        }

    }
}