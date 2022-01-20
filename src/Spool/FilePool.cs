using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spool.IO;

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

        private CancellationTokenSource _cancellationTokenSource;

        public bool IsRunning { get; private set; } = false;
        public FilePoolConfiguration Configuration { get; }

        private readonly ILogger _logger;
        public FilePool(
            ILogger<FilePool> logger,
            FilePoolConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;

            _returnFileScanStartDelayMillis = Configuration.ReturnFileScanStartDelayMillis;
            _returnFileScanMillis = Configuration.ReturnFileScanMillis;
            _fileWatcherScanIntervalMillis = Configuration.FileWatcherScanIntervalMillis;
            _fileWatcherWorkThread = Configuration.FileWatcherWorkThread;

            _cancellationTokenSource = new CancellationTokenSource();
        }


        public void Start()
        {
            if (IsRunning)
            {
                _logger.LogInformation("FilePool {Name} is running, don't run it repeat!", Configuration.Name);
                return;
            }

            _logger.LogInformation("");

            IsRunning = true;
        }

        public void Shutdown()
        {
            _cancellationTokenSource?.Cancel();
            IsRunning = false;
        }


        private void Setup()
        {

            if (DirectoryHelper.CreateIfNotExists(Configuration.Path))
            {
                _logger.LogDebug("Create filePool {Name} directory '{Path}'.", Configuration.Name, Configuration.Path);
            }

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
                        await WriteWatcherFileAsync(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Write file from '{0}' failed, exception:{1}.", file, ex.Message);
                    }
                }
            }

        }

        private async Task WriteWatcherFileAsync(string file)
        {
            if (PathUtil.IsSamePathRoot(file, Configuration.Path))
            {
                //Move in

            }
            else
            {
                //Write
            }
        }




    }
}