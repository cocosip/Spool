using Microsoft.Extensions.Logging;
using Spool.Events;
using Spool.Trains;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>
    /// File pool
    /// </summary>
    /// <typeparam name="TFilePool"></typeparam>
    public class FilePool<TFilePool> : IFilePool<TFilePool> where TFilePool : class
    {
        private readonly IFilePool _filePool;

        /// <summary>
        /// ctor
        /// </summary>
        public FilePool(IFilePoolFactory filePoolFactory)
        {
            _filePool = filePoolFactory.GetOrCreate<TFilePool>();
        }

        /// <summary>
        /// Setup
        /// </summary>
        public void Setup()
        {
            _filePool.Setup();
        }

        /// <summary>
        /// Gets the specified number of files
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<SpoolFile> GetFiles(int count = 1)
        {
            return _filePool.GetFiles(count);
        }

        /// <summary>
        /// Write file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public Task<SpoolFile> WriteFileAsync(Stream stream, string fileExt)
        {
            return _filePool.WriteFileAsync(stream, fileExt);
        }

        /// <summary>
        /// Return files to file pool
        /// </summary>
        /// <param name="files"></param>
        public void ReturnFiles(params SpoolFile[] files)
        {
            _filePool.ReturnFiles(files);
        }

        /// <summary>
        /// Release files
        /// </summary>
        /// <param name="files"></param>
        public void ReleaseFiles(params SpoolFile[] files)
        {
            _filePool.ReleaseFiles(files);
        }

        /// <summary>
        /// Get pending files count
        /// </summary>
        /// <returns></returns>
        public int GetPendingCount()
        {
            return _filePool.GetPendingCount();
        }

        /// <summary>
        /// Get processing files count
        /// </summary>
        /// <returns></returns>
        public int GetProcessingCount()
        {
            return _filePool.GetProcessingCount();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            //do nothing
        }
    }

    /// <summary>
    /// File pool
    /// </summary>
    public class FilePool : IFilePool
    {
        private bool _isSetup = false;
        private readonly ManualResetEventSlim _sync;

        private readonly ILogger _logger;
        private readonly IScheduleService _scheduleService;
        private readonly ITrainFactory _trainFactory;

        private readonly string _autoReturnFilesTaskName = "Spool.AutoReturnFiles";
        private readonly string _fileWatcherTaskName = "Spool.FileWatcher";

        private readonly ConcurrentDictionary<int, ITrain> _trainDict;
        private readonly ConcurrentDictionary<string, SpoolFileFuture> _processingFileDict;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="scheduleService"></param>
        /// <param name="trainFactory"></param>
        public FilePool(ILogger<FilePool> logger, FilePoolConfiguration configuration, IScheduleService scheduleService, ITrainFactory trainFactory)
        {
            _logger = logger;
            Configuration = configuration;
            _scheduleService = scheduleService;
            _trainFactory = trainFactory;

            _sync = new ManualResetEventSlim(true);

            _autoReturnFilesTaskName = $"{_autoReturnFilesTaskName}.{configuration.Name}";
            _fileWatcherTaskName = $"{_fileWatcherTaskName}.{configuration.Name}";

            _trainDict = new ConcurrentDictionary<int, ITrain>();
            _processingFileDict = new ConcurrentDictionary<string, SpoolFileFuture>();
        }

        /// <summary>
        /// File pool configuration
        /// </summary>
        public FilePoolConfiguration Configuration { get; }

        /// <summary>
        /// Setup file pool
        /// </summary>
        public void Setup()
        {
            if (_isSetup)
            {
                _logger.LogDebug("File pool '{0}' has been setuped!", Configuration.Name);
                return;
            }

            //Create current file pool 
            if (FilePathUtil.CreateIfNotExists(Configuration.Path))
            {
                _logger.LogDebug("Create file pool '{0}' file directory '{1}'.", Configuration.Name, Configuration.Path);
            }

            //Initialize trains
            InitializeTrains();

            //AutoReturnFiles
            if (Configuration.EnableAutoReturn)
            {
                StartScanReturnFiles();
            }

            //FileWatcher
            if (Configuration.EnableFileWatcher)
            {
                if (string.IsNullOrWhiteSpace(Configuration.FileWatcherPath))
                {
                    throw new ArgumentException("FileWatcherPath was null!");
                }
                if (FilePathUtil.CreateIfNotExists(Configuration.FileWatcherPath))
                {
                    _logger.LogInformation("Creat file pool '{0}' file watcher in '{1}'.", Configuration.Name, Configuration.FileWatcherPath);
                }

                StartScanFileWatcher();
            }

            _logger.LogDebug("File pool '{0}' setup complete !", Configuration.Name);
            _isSetup = true;
        }

        /// <summary>
        /// Gets the specified number of files
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<SpoolFile> GetFiles(int count = 1)
        {
            var train = GetReadTrain();
            var spoolFiles = train.GetFiles(count);
            if (spoolFiles.Count < count)
            {
                var secondTrain = GetReadTrain();
                if (secondTrain != null)
                {
                    var secondSpoolFiles = secondTrain.GetFiles(count - spoolFiles.Count);
                    spoolFiles.AddRange(secondSpoolFiles);
                }
            }

            //Enable auto reutn
            if (Configuration.EnableAutoReturn)
            {
                foreach (var spoolFile in spoolFiles)
                {
                    var key = spoolFile.GenerateCode();
                    if (_processingFileDict.ContainsKey(key))
                    {
                        _logger.LogDebug("Processing file dict has contain this file '{0}'.", key);
                    }
                    else
                    {
                        var spoolFileFuture = new SpoolFileFuture(spoolFile, Configuration.AutoReturnSeconds);
                        if (!_processingFileDict.TryAdd(spoolFile.GenerateCode(), spoolFileFuture))
                        {
                            _logger.LogWarning("File pool add processing file failed ,{0}.", spoolFile);
                        }
                    }
                }
            }
            return spoolFiles;
        }

        /// <summary>
        /// Write file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteFileAsync(Stream stream, string fileExt)
        {
            var train = GetWriteTrain();
            return await train.WriteFileAsync(stream, fileExt);
        }

        /// <summary>
        /// Return files to file pool
        /// </summary>
        /// <param name="files"></param>
        public void ReturnFiles(params SpoolFile[] files)
        {
            var groupSpoolFiles = files.GroupBy(x => x.TrainIndex);
            foreach (var groupSpoolFile in groupSpoolFiles)
            {
                var train = GetTrainByIndex(groupSpoolFile.Key);
                if (train != null)
                {
                    train.ReturnFiles(groupSpoolFile.ToArray());
                }
                else
                {
                    _logger.LogWarning("Return file '{0}' failed, it may be removed。", groupSpoolFile.Key);
                }
            }

            if (Configuration.EnableAutoReturn)
            {
                TryRemoveProcessingFiles(files);
            }
        }

        /// <summary>
        ///  Release files
        /// </summary>
        /// <param name="files"></param>
        public void ReleaseFiles(params SpoolFile[] files)
        {
            var groupSpoolFiles = files.GroupBy(x => x.TrainIndex);
            foreach (var groupSpoolFile in groupSpoolFiles)
            {
                var train = GetTrainByIndex(groupSpoolFile.Key);
                if (train != null)
                {
                    train.ReleaseFiles(groupSpoolFile.ToArray());
                }
                else
                {
                    _logger.LogWarning("Release file '{0}' failed, it may be removed。", groupSpoolFile.Key);
                }
            }

            if (Configuration.EnableAutoReturn)
            {
                TryRemoveProcessingFiles(files);
            }
        }




        /// <summary>
        /// Get pending files count
        /// </summary>
        /// <returns></returns>
        public int GetPendingCount()
        {
            var trains = _trainDict.Values
                .Where(x => x.TrainType == TrainType.Read || x.TrainType == TrainType.ReadWrite)
                .ToList();
            return trains.Sum(x => x.PendingCount);
        }

        /// <summary>
        /// Get processing files count
        /// </summary>
        /// <returns></returns>
        public int GetProcessingCount()
        {
            return _processingFileDict.Count;
        }

        #region Private methods

        /// <summary>
        /// Initialize trains
        /// </summary>
        private void InitializeTrains()
        {
            _logger.LogDebug("File pool '{0}' initialize trains begin ...", Configuration.Name);
            var directoryInfo = new DirectoryInfo(Configuration.Path);
            var trainDirs = directoryInfo.GetDirectories();
            foreach (var trainDir in trainDirs)
            {
                if (TrainUtil.IsTrainName(trainDir.Name))
                {
                    var index = TrainUtil.GetTrainIndex(trainDir.Name);
                    var train = _trainFactory.Create(Configuration, index);
                    _trainDict.AddOrUpdate(index, train, (k, v) => train);
                }
            }

            //train status
            if (_trainDict.Count == 0)
            {
                var train = _trainFactory.Create(Configuration, 1);
                if (!_trainDict.TryAdd(train.Index, train))
                {
                    throw new ArgumentException($"Create train failed for file pool '{Configuration.Name}', train '1' .");
                }
            }

            //Bind default events
            foreach (var train in _trainDict.Values)
            {
                BindDefaultEvents(train);
                train.Initialize();
            }

            //Write and read is same train
            if (_trainDict.Count == 1)
            {
                var train = _trainDict.Values.FirstOrDefault();
                train.ChangeType(TrainType.ReadWrite);
            }

            //There are more than 2 train before
            if (_trainDict.Count >= 2)
            {
                var latest = _trainDict.Values.OrderByDescending(x => x.Index).FirstOrDefault();
                latest.ChangeType(TrainType.Write);

                var first = _trainDict.Values.OrderBy(x => x.Index).FirstOrDefault();
                first.ChangeType(TrainType.Read);
            }

            _logger.LogDebug("File pool '{0}' initialize trains end ...", Configuration.Name);

        }

        /// <summary>
        /// Bind train default event
        /// </summary>
        /// <param name="train"></param>
        private void BindDefaultEvents(ITrain train)
        {
            train.OnDelete += Train_OnDelete;
            train.OnWriteOver += Train_OnWriteOver;
        }

        /// <summary>
        /// Unbind train default event
        /// </summary>
        /// <param name="train"></param>
        private void UnBindDefaultEvents(ITrain train)
        {
            train.OnDelete -= Train_OnDelete;
            train.OnWriteOver -= Train_OnWriteOver;
        }

        /// <summary>
        /// Train write over
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Train_OnWriteOver(object sender, TrainWriteOverEventArgs e)
        {
            _sync.Wait();

            try
            {
                _sync.Reset();

                if (_trainDict.TryGetValue(e.Train.Index, out ITrain train))
                {
                    //Readwrite -->Read
                    //Write --> Read
                    if (train.TrainType == TrainType.ReadWrite || train.TrainType == TrainType.Write)
                    {
                        train.ChangeType(TrainType.Read);
                    }
                    else
                    {
                        _logger.LogInformation("Current train type change to read ,but original type was not 'ReadWrite' or 'Write'.");
                    }

                    //Create new train
                    var nextIndex = GetLatestNextIndex();
                    var newWriteTrain = _trainFactory.Create(Configuration, nextIndex);
                    _trainDict.TryAdd(newWriteTrain.Index, newWriteTrain);

                    BindDefaultEvents(newWriteTrain);
                    newWriteTrain.Initialize();

                    //Set train type to write
                    newWriteTrain.ChangeType(TrainType.Write);
                }
                else
                {
                    _logger.LogInformation("Can't find train when train write over!");
                }

            }
            finally
            {
                _sync.Set();
            }
        }

        /// <summary>
        /// Train on delete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Train_OnDelete(object sender, TrainDeleteEventArgs e)
        {
            _sync.Wait();
            try
            {
                _sync.Reset();

                //Remove train from dict
                if (_trainDict.TryRemove(e.Train.Index, out ITrain train))
                {
                    try
                    {
                        var files = Directory.GetFiles(e.Train.Path);
                        if (files.Count() > 0)
                        {
                            throw new Exception($"Delete train , there are still {files.Count()} files in train,could not delete!");
                        }


                        //Delete train files
                        FilePathUtil.DeleteDirIfExist(e.Train.Path, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Delete train from local path failed,ex:{0}", ex.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Delete train files failed, file pool '{0}', train:'{1}'.", Configuration.Name, e.Train.Index);
                }

                //Unbind events
                if (train != null)
                {
                    UnBindDefaultEvents(train);
                }
            }
            finally
            {
                _sync.Set();
            }

        }

        /// <summary>
        /// Get next train index
        /// </summary>
        /// <returns></returns>
        private int GetLatestNextIndex()
        {
            var latestIndex = _trainDict.Keys.OrderByDescending(x => x).FirstOrDefault();
            return latestIndex + 1;
        }

        /// <summary>
        /// Try remove processing fails
        /// </summary>
        private void TryRemoveProcessingFiles(params SpoolFile[] spoolFiles)
        {
            foreach (var spoolFile in spoolFiles)
            {
                //The file may has been removed by auto return
                _processingFileDict.TryRemove(spoolFile.GenerateCode(), out SpoolFileFuture _);
            }
        }

        #endregion

        #region Trains

        /// <summary>
        /// Get write train
        /// </summary>
        public ITrain GetWriteTrain()
        {
            _sync.Wait();

            var writeTrain = _trainDict.Values.FirstOrDefault(x => x.TrainType == TrainType.Write || x.TrainType == TrainType.ReadWrite);
            if (writeTrain == null)
            {
                //No write train
                try
                {
                    _sync.Reset();
                    writeTrain = _trainFactory.Create(Configuration, GetLatestNextIndex());

                    BindDefaultEvents(writeTrain);
                    writeTrain.Initialize();

                    writeTrain.ChangeType(TrainType.Write);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "File pool '{0}', create new write train failed,Exception:{1}.", Configuration.Name, ex.Message);
                    throw ex;
                }
                finally
                {
                    _sync.Set();
                }
            }
            return writeTrain;
        }

        /// <summary>
        /// Get read train
        /// </summary>
        private ITrain GetReadTrain()
        {
            _sync.Wait();

            //Get read train first
            var readTrain = _trainDict.Values.FirstOrDefault(x => x.TrainType == TrainType.Read && !x.IsPendingEmpty());
            if (readTrain == null)
            {
                //Get default train
                readTrain = _trainDict.Values.OrderBy(x => x.Index).FirstOrDefault(x => x.TrainType == TrainType.Default);

                //load files
                if (readTrain != null)
                {
                    _sync.Wait();

                    try
                    {
                        _sync.Reset();
                        //To read type
                        readTrain.ChangeType(TrainType.Read);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "File pool '{0}',convert 'default' type train to 'read' type failed,Exception:{1}.", Configuration.Name, ex.Message);
                    }
                    finally
                    {
                        _sync.Set();
                    }
                }
            }

            //Get readwrite train
            if (readTrain == null)
            {
                readTrain = _trainDict.Values.FirstOrDefault(x => x.TrainType == TrainType.ReadWrite);
            }

            if (readTrain == null)
            {
                _sync.Wait();

                //Get a write train
                var writeTrain = _trainDict.Values.OrderByDescending(x => x.Index).FirstOrDefault(x => x.TrainType == TrainType.Write);

                if (writeTrain != null)
                {
                    try
                    {
                        _sync.Reset();
                        writeTrain.ChangeType(TrainType.ReadWrite);
                        return writeTrain;
                    }
                    finally
                    {
                        _sync.Set();
                    }
                }
            }

            if (readTrain == null)
            {
                throw new ArgumentException("Can't find any read train!");
            }
            return readTrain;
        }

        /// <summary>
        /// Get train by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ITrain GetTrainByIndex(int index)
        {
            _sync.Wait();
            if (!_trainDict.TryGetValue(index, out ITrain train))
            {
                _logger.LogWarning("Get train '{0}' faile,this train was not exist or was released.", index);
            }
            return train;
        }

        #endregion

        /// <summary>
        /// Scan time out files
        /// </summary>
        private void StartScanReturnFiles()
        {
            _scheduleService.StartTask(_autoReturnFilesTaskName, () =>
            {
                try
                {
                    var timeoutKeyList = new List<string>();
                    foreach (var entry in _processingFileDict)
                    {
                        if (entry.Value.IsTimeout())
                        {
                            timeoutKeyList.Add(entry.Key);
                        }
                    }
                    foreach (var key in timeoutKeyList)
                    {

                        if (_processingFileDict.TryRemove(key, out SpoolFileFuture spoolFileFuture))
                        {
                            ReturnFiles(spoolFileFuture.File);
                            _logger.LogDebug("Auto return file:{0}", spoolFileFuture.File);
                        }
                        else
                        {
                            _logger.LogWarning("Remove expired file failed ,'{0}'", key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scan return files caught exception,{0}", ex.Message);
                }
            }, 5000, Configuration.ScanReturnFileMillSeconds);
        }

        /// <summary>
        /// Enable file watcher
        /// </summary>
        private void StartScanFileWatcher()
        {
            _scheduleService.StartTask(_fileWatcherTaskName, async () =>
            {
                var deleteFiles = new List<string>();
                try
                {
                    var files = FilePathUtil.RecursiveGetFileInfos(Configuration.FileWatcherPath);
                    foreach (var file in files)
                    {
                        //Last write time 2s ago
                        if (file.LastAccessTime < DateTime.Now.AddSeconds(-2))
                        {
                            using var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                            var ext = FilePathUtil.GetPathExtension(file.Name);
                            await WriteFileAsync(fs, ext);
                            //Add to delete path
                            deleteFiles.Add(file.FullName);
                            _logger.LogDebug("Watcher file '{0}' was written in '{1}'.", file.FullName, Configuration.Name);

                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "File watcher exception:{0}.", ex.Message);
                    //throw ex;
                }
                finally
                {
                    try
                    {
                        if (deleteFiles.Any())
                        {
                            foreach (var deleteFile in deleteFiles)
                            {
                                FilePathUtil.DeleteFileIfExists(deleteFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Delete watcher file exception:{0}.", ex.Message);
                    }
                }
            }, 5000, Configuration.ScanFileWatcherMillSeconds);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _scheduleService.StopTask(_autoReturnFilesTaskName);
            _scheduleService.StopTask(_fileWatcherTaskName);
            _sync?.Dispose();
        }
    }
}
