using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spool.Utils;
using Spool.Workers;

namespace Spool
{
    public class FilePool<TFilePool> : IFilePool<TFilePool> where TFilePool : class
    {
        public FilePool()
        {



        }
    }


    public class FilePool : IFilePool
    {

        private int _returnFileScanStartDelayMillis = 5000;
        private int _returnFileScanMillis = 3000;

        private int _fileWatcherScanStartDelayMillis = 5000;
        private int _fileWatcherScanIntervalMillis = 5000;
        private int _fileWatcherWorkThread = 3;

        private readonly object _writeLock = new object();
        private readonly object _readLock = new object();
        private int _writeWorker = 0;
        private CancellationTokenSource _cancellationTokenSource;
        private ConcurrentDictionary<int, IWorker> _workerDict;

        public bool IsRunning { get; private set; } = false;
        public FilePoolConfiguration Configuration { get; }

        private readonly ILogger _logger;
        private readonly IWorkerFactory _workerFactory;
        public FilePool(
            ILogger<FilePool> logger,
            FilePoolConfiguration configuration,
            IWorkerFactory workerFactory)
        {
            _logger = logger;
            Configuration = configuration;
            _workerFactory = workerFactory;

            _returnFileScanStartDelayMillis = Configuration.ReturnFileScanStartDelayMillis;
            _returnFileScanMillis = Configuration.ReturnFileScanMillis;
            _fileWatcherScanIntervalMillis = Configuration.FileWatcherScanIntervalMillis;
            _fileWatcherWorkThread = Configuration.FileWatcherWorkThread;

            _cancellationTokenSource = new CancellationTokenSource();
            _workerDict = new ConcurrentDictionary<int, IWorker>();
        }


        public void Start()
        {
            if (IsRunning)
            {
                _logger.LogInformation("FilePool {Name} is running, don't run it repeat!", Configuration.Name);
                return;
            }

            Setup();
            IsRunning = true;

            _logger.LogDebug("FilePool {0} start...", Configuration.Name);
        }

        public void Shutdown()
        {
            _cancellationTokenSource?.Cancel();
            IsRunning = false;
            _logger.LogDebug("FilePool {0} shutdown...", Configuration.Name);
        }

        private void Setup()
        {
            if (PathUtil.CreateIfNotExists(Configuration.Path))
            {
                _logger.LogDebug("Create filePool {Name} directory '{Path}'.", Configuration.Name, Configuration.Path);
            }

            //Load workers
            LoadWorkers();

            if (Configuration.EnableAutoReturn)
            {
                StartScanReturnFiles();
                _logger.LogDebug("FilePool {Name} start scan return files...", Configuration.Name);
            }

            if (Configuration.EnableFileWatcher)
            {
                if (string.IsNullOrWhiteSpace(Configuration.FileWatcherPath))
                {
                    throw new ArgumentNullException("FileWatcher is enabled, but fileWatcherPath is empty!");
                }

                StartScanFileWatcher();
                _logger.LogDebug("FilePool {Name} start scan file watcher...", Configuration.Name);
            }
        }



        private void LoadWorkers()
        {
            var workers = new List<IWorker>();
            var directoryInfo = new DirectoryInfo(Configuration.Path);
            var workerDirs = directoryInfo.GetDirectories();

            foreach (var workerDir in workerDirs)
            {
                if (WorkerUtil.IsWorkerName(workerDir.Name))
                {
                    var number = WorkerUtil.ParseNumber(workerDir.Name);
                    var worker = _workerFactory.CreateWorker(Configuration, number);
                    workers.Add(worker);
                }
                else
                {
                    _logger.LogDebug("Directory {Name} is not a worker dir.", workerDir.Name);
                }
            }

            if (!workers.Any())
            {
                //Create first worker
                var worker = _workerFactory.CreateWorker(Configuration, 1);
                workers.Add(worker);
            }

            // foreach (var worker in workers)
            // {
            //     if (!worker.IsSetup)
            //     {
            //         worker.Setup();
            //     }
            //     if (_workerDict.TryAdd(worker.Number, worker))
            //     {
            //         _logger.LogDebug("Add worker {Name} to worker dict failed.", worker.Name);
            //     }
            // }

            //TODO state change?
            if (workers.Count == 1)
            {
                var worker = workers.FirstOrDefault();
                if (!worker.IsSetup)
                {
                    worker.Setup();
                }
            }
            else if (workers.Count > 1)
            {

            }
        }


        public async Task<SpoolFile> WriteAsync(Stream stream, string ext)
        {
            var worker = GetWriteWorker();
            return await worker.WriteAsync(stream, ext);
        }


        public List<SpoolFile> Get(int count = 1)
        {

            return default;
        }


        private IWorker GetWriteWorker()
        {
            var currentWorkerNumber = _writeWorker;
            if (_workerDict.TryGetValue(currentWorkerNumber, out IWorker worker))
            {
                if (worker.TryEntryWrite())
                {
                    return worker;
                }
                else
                {
                    if (currentWorkerNumber != _writeWorker)
                    {
                        if (_workerDict.TryGetValue(_writeWorker, out worker))
                        {
                            if (worker.TryEntryWrite())
                            {
                                return worker;
                            }
                        }
                    }
                }
            }

            _logger.LogDebug("Get write worker by worker number {_writeWorker} failed.", _writeWorker);
            throw new Exception("Could not get any write worker.");
        }

        private IWorker GetReadWorker()
        {
            var worker = _workerDict.Values.FirstOrDefault(x => x.State == WorkerState.Read || x.State == WorkerState.ReadWrite && !x.IsPendingEmpty());
            if (worker == null)
            {
                worker = _workerDict.Values.Where(x => x.State == WorkerState.Pending).OrderBy(x => x.Number).FirstOrDefault();
                worker.ChangeState(WorkerState.Read);
            }

            return worker;
        }

        private void WriteFull(IWorker worker, WorkerState oldState, WorkerState newState)
        {
            lock (_writeLock)
            {
                if (newState == WorkerState.Read)
                {
                    var nextWorkerNumber = GetNextWorkerNumber();
                    var nextWorker = _workerFactory.CreateWorker(Configuration, nextWorkerNumber);
                    nextWorker.Setup();
                    nextWorker.ChangeState(WorkerState.Write);

                    if (_workerDict.TryAdd(nextWorkerNumber, nextWorker))
                    {
                        _writeWorker = nextWorkerNumber;
                    }
                    else
                    {
                        _logger.LogDebug("Add new worker {nextWorkerNumber} to dict failed.", nextWorkerNumber);
                    }
                }

            }
        }
        private int GetNextWorkerNumber()
        {
            var max = _workerDict.Values.Max(x => x.Number);
            return max++;
        }
        private void StartScanReturnFiles()
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(_returnFileScanStartDelayMillis, _cancellationTokenSource.Token);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Scan return files has exception:{Message}.", ex.Message);
                        await Task.Delay(200);
                    }

                    await Task.Delay(_returnFileScanMillis, _cancellationTokenSource.Token);

                }

            }, TaskCreationOptions.LongRunning);

        }

        private void StartScanFileWatcher()
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(_fileWatcherScanStartDelayMillis, _cancellationTokenSource.Token);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {

                    try
                    {
                        var pendingFiles = new List<string>();
                        var fileInfos = PathUtil.RecursiveGetFileInfos(Configuration.FileWatcherPath);
                        foreach (var fileInfo in fileInfos)
                        {
                            if (Configuration.FileWatcherSkipZeroFile && fileInfo.Length <= 0)
                            {
                                _logger.LogDebug("Skip zero size file {FullName}", fileInfo.FullName);
                                continue;
                            }

                            if (fileInfo.LastWriteTime < DateTime.Now.AddSeconds(-Configuration.FileWatcherReadDelaySeconds))
                            {
                                pendingFiles.Add(fileInfo.FullName);
                            }
                        }

                        if (pendingFiles.Any())
                        {

                            await WriteWatcherFileToPoolAsync(pendingFiles);
                        }
                        else
                        {
                            _logger.LogDebug("Scan file watcher files is empty in {FileWatcherPath}.", Configuration.FileWatcherPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Scan file watcher has exception:{Message}.", ex.Message);
                        await Task.Delay(200);
                    }

                    await Task.Delay(_fileWatcherScanIntervalMillis, _cancellationTokenSource.Token);


                }

            }, TaskCreationOptions.LongRunning);
        }

        private async Task WriteWatcherFileToPoolAsync(List<string> files)
        {
            if (files.Count >= _fileWatcherWorkThread)
            {
                var queue = new ConcurrentQueue<string>(files);
                var tasks = new List<Task>();

                for (var i = 0; i < _fileWatcherWorkThread; i++)
                {
                    var task = Task.Factory.StartNew(async () =>
                    {

                        while (!queue.IsEmpty)
                        {
                            try
                            {
                                if (queue.TryDequeue(out string file))
                                {

                                }
                                else
                                {
                                    _logger.LogDebug("File watcher try dequeue failed.");
                                }

                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Write file watcher file to spool failed. {Message}", ex.Message);
                            }

                        }


                    }, _cancellationTokenSource.Token);

                    tasks.Add(task);
                }

            }
            else
            {
                foreach (var file in files)
                {
                    try
                    {
                        var worker = GetWriteWorker();
                        await worker.WriteAsync(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Write file from '{0}' failed, exception:{1}.", file, ex.Message);
                    }
                }
            }

        }


    }
}