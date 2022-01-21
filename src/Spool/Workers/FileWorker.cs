using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spool.Utils;

namespace Spool.Workers
{
    public class FileWorker : IWorker
    {
        private bool _isLoaded = false;
        private int _workerMaxFile = 2000;
        private int _writingCount = 0;
        private FilePoolConfiguration _configuration;
        private Action<int, WorkerState, WorkerState> WriteFullFunc = null;

        private readonly object _writeSync = new object();


        private readonly ConcurrentQueue<SpoolFile> _pendingQueue;
        private readonly ConcurrentDictionary<string, SpoolFile> _progressingDict;

        public bool IsSetup { get; private set; } = false;
        public string Name { get; private set; }
        public int Number { get; private set; }
        public string Path { get; private set; }
        public bool IsLoaded { get; private set; }

        public WorkerState State { get; private set; }

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
            Path = System.IO.Path.Combine(_configuration.Path, Name);
            State = WorkerState.Pending;

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

            if (PathUtil.CreateIfNotExists(Path))
            {
                _logger.LogInformation("Worker {Name}(Name) create dir {Path}.", _configuration.Name, Name, Path);
            }
            _logger.LogDebug("Worker {Name}({Name}) setup.", _configuration.Name, Name);
        }

        public void Load()
        {
            if (IsLoaded)
            {
                _logger.LogDebug("Worker {Name}(Name) is loaded, don't load repeat!", _configuration.Name, Name);
                return;
            }

            var files = Directory.GetFiles(Path);
            foreach (var file in files)
            {
                var spoolFile = new SpoolFile(_configuration.Name, Number, file);
                if (!_pendingQueue.TryDequeue(out spoolFile))
                {
                    _logger.LogDebug("Add worker({Name}) file {file} to pending queue failed.", Name, file);
                }
            }

            IsLoaded = true;
            _logger.LogDebug("Load worker {0} complete.", Name);
        }

        public void ChangeState(WorkerState state)
        {
            if (State == WorkerState.Pending && state != WorkerState.Pending)
            {
                Load();
            }
            State = state;
        }

        public bool IsPendingEmpty()
        {
            return _pendingQueue.IsEmpty;
        }

        public int GetPendingCount()
        {
            return _pendingQueue.Count;
        }

        public int GetProcessingCount()
        {
            return _progressingDict.Count;
        }

        public bool NextIsFull()
        {
            return _pendingQueue.Count + _progressingDict.Count + _writingCount + 1 >= _workerMaxFile;
        }

        public bool TryEntryWrite()
        {
            lock (_writeSync)
            {
                if (NextIsFull())
                {
                    var oldState = State;
                    var newState = State;

                    //Change state
                    if (State == WorkerState.Write || State == WorkerState.ReadWrite)
                    {
                        ChangeState(WorkerState.Read);
                    }
                    else
                    {
                        _logger.LogDebug("Worker(WriteFull) {Name} state {State} incorrect.", Name, State);
                    }
                    WriteFullFunc?.Invoke(Number, oldState, newState);
                    return false;
                }
                else
                {
                    _writingCount++;
                    return true;
                }
            }
        }

        public async Task<SpoolFile> WriteAsync(Stream stream, string ext)
        {
            var spoolFile = new SpoolFile(_configuration.Name, Number);
            try
            {
                var filePath = System.IO.Path.Combine(Path, $"{ObjectId.GenerateNewStringId()}{ext}");
                using (var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs);
                }
                spoolFile.FilePath = filePath;

                _pendingQueue.Enqueue(spoolFile);
            }
            finally
            {
                _writingCount--;
            }

            return spoolFile;
        }

        public async Task<SpoolFile> WriteAsync(string fileName)
        {
            if (PathUtil.IsSamePathRoot(fileName, Path))
            {
                return MoveFileInternal(fileName);
            }
            else
            {
                var ext = System.IO.Path.GetExtension(fileName);
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return await WriteAsync(fs, ext);
                }
            }
        }

        public List<SpoolFile> Get(int count)
        {
            var spoolFiles = new List<SpoolFile>();
            for (var i = 0; i < count; i++)
            {
                if (_pendingQueue.TryDequeue(out SpoolFile spoolFile))
                {
                    var key = spoolFile.AsKey();

                    if (_progressingDict.TryAdd(key, spoolFile))
                    {
                        spoolFiles.Add(spoolFile);
                    }
                    else
                    {
                        _logger.LogDebug("Add worker file to processing dict failed.");
                    }
                }
                else
                {
                    break;
                }
            }

            return spoolFiles;
        }


        public void ReturnFiles(List<SpoolFile> spoolFiles)
        {
            foreach (var spoolFile in spoolFiles)
            {
                var key = spoolFile.AsKey();
                if (_progressingDict.TryRemove(key, out SpoolFile _))
                {
                    _pendingQueue.Enqueue(spoolFile);
                }
                else
                {
                    _logger.LogDebug("Try remove spool file {key} failed.", key);
                }
            }
        }

        public void ReleaseFiles(List<SpoolFile> spoolFiles)
        {
            foreach (var spoolFile in spoolFiles)
            {
                var key = spoolFile.AsKey();
                if (_progressingDict.TryRemove(key, out SpoolFile removeFile))
                {
                    try
                    {
                        PathUtil.DeleteIfExists(spoolFile.FilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Release file {FilePath} failed. {Message}", spoolFile.FilePath, ex.Message);
                        if (!_progressingDict.TryAdd(key, removeFile))
                        {
                            _logger.LogDebug("ReAdd release file to dict failed.");
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("Try remove spool file {key} failed.", key);
                }
            }

            if (_pendingQueue.IsEmpty && _progressingDict.IsEmpty)
            {
                //

            }

        }

        private SpoolFile MoveFileInternal(string fileName)
        {
            var spoolFile = new SpoolFile(_configuration.Name, Number);
            try
            {
                var ext = System.IO.Path.GetExtension(fileName);
                var filePath = System.IO.Path.Combine(Path, $"{ObjectId.GenerateNewStringId()}{ext}");
                File.Move(fileName, filePath);
                spoolFile.FilePath = filePath;
                _pendingQueue.Enqueue(spoolFile);
            }
            finally
            {
                _writingCount--;
            }

            return spoolFile;

        }


    }
}