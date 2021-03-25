using Microsoft.Extensions.Logging;
using Spool.Events;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Spool.Trains
{
    /// <summary>
    /// Train
    /// </summary>
    public class Train : ITrain
    {
        /// <summary>
        /// Delete train event
        /// </summary>
        public event EventHandler<TrainDeleteEventArgs> OnDelete;

        /// <summary>
        /// Train type change event
        /// </summary>
        public event EventHandler<TrainTypeChangeEventArgs> OnTypeChange;

        /// <summary>
        /// Train write over event
        /// </summary>
        public event EventHandler<TrainWriteOverEventArgs> OnWriteOver;

        /// <summary>
        /// File pool name
        /// </summary>
        public string FilePool => _configuration?.Name;

        /// <summary>
        /// Train name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Train path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Train index
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// TrainType
        /// </summary>
        public TrainType TrainType { get; private set; }

        /// <summary>
        /// Pending handle files
        /// </summary>
        public int PendingCount { get { return _pendingQueue.Count; } }

        /// <summary>
        /// Take away to handle files
        /// </summary>
        public int ProgressingCount { get { return _progressingDict.Count; } }

        private readonly ILogger _logger;
        private readonly FilePoolConfiguration _configuration;

        private bool _initialized = false;
        private readonly ConcurrentQueue<SpoolFile> _pendingQueue;
        private readonly ConcurrentDictionary<string, SpoolFile> _progressingDict;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="index"></param>
        public Train(
            ILogger<Train> logger,
            FilePoolConfiguration configuration,
            int index)
        {
            _logger = logger;
            _configuration = configuration;

            Name = TrainUtil.GenerateTrainName(index);
            Path = TrainUtil.GenerateTrainPath(configuration.Path, Name);
            Index = index;
            TrainType = TrainType.Default;

            _pendingQueue = new ConcurrentQueue<SpoolFile>();
            _progressingDict = new ConcurrentDictionary<string, SpoolFile>();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                _logger.LogDebug("The train '{0}' in  file pool '{1}' has been initialized .", _configuration.Name, Name);
                return;
            }

            //Create train directory
            if (FilePathUtil.CreateIfNotExists(Path))
            {
                _logger.LogDebug("Create train directory, [FilePool:'{0}',FilePool Path:'{1}',Train :'{2}'].", _configuration.Name, _configuration.Path, Index);
            }
            else
            {
                _logger.LogDebug("Create train '{0}' directory failed in  file pool '{1}'!", _configuration.Name, Name);
            }
            _initialized = true;
        }

        /// <summary>
        /// Train info
        /// </summary>
        public string Info()
        {
            return $"[FilePool:{_configuration.Name},FilePool path:{_configuration.Path},Index:{Index},Train name:{Name},Train path:{Path}]";
        }

        /// <summary>
        /// Whether pending queue is empty
        /// </summary>
        /// <returns></returns>
        public bool IsPendingEmpty()
        {
            return _pendingQueue.IsEmpty;
        }

        /// <summary>
        /// Write file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public async ValueTask<SpoolFile> WriteFileAsync(Stream stream, string fileExt)
        {
            var spoolFile = new SpoolFile(_configuration.Name, Index);
            try
            {
                var path = GenerateFilePath(fileExt);
                spoolFile.Path = path;
                //Write file
                await WriteInternalAsync(stream, path);

                //Write queue
                _pendingQueue.Enqueue(spoolFile);

                //是否写满了(需要按照待处理的文件数量+处理中的数量进行计算,避免当关闭自动归还功能时,磁盘下的文件还有大量的堆积)
                if (_pendingQueue.Count + _progressingDict.Count > _configuration.TrainMaxFileCount)
                {
                    var info = BuildInfo();
                    var args = new TrainWriteOverEventArgs()
                    {
                        Train = info,
                    };
                    OnWriteOver?.Invoke(this, args);
                }

                return spoolFile;
            }
            catch (Exception ex)
            {
                _logger.LogError("Write file to train failed, FilePool:'{0}',Train:'{1}',FileExt:'{2}',Exception:{3}.", _configuration.Name, Index, fileExt, ex.Message);
                throw ex;
            }
            finally
            {
                //流释放
                stream?.Close();
                stream?.Dispose();
            }
        }


        /// <summary>
        /// Gets the specified number of files
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<SpoolFile> GetFiles(int count = 1)
        {
            var spoolFiles = new List<SpoolFile>();
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (!_pendingQueue.IsEmpty)
                    {
                        _pendingQueue.TryDequeue(out SpoolFile spoolFile);
                        if (spoolFile != null)
                        {
                            spoolFiles.Add(spoolFile);
                            _progressingDict.TryAdd(spoolFile.GenerateCode(), spoolFile);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("From train:'{0}' get file failed。Exception:{1}", Index, ex.Message);
                throw;
            }
            return spoolFiles;
        }

        /// <summary>
        /// Return files
        /// </summary>
        /// <param name="spoolFiles"></param>
        public void ReturnFiles(params SpoolFile[] spoolFiles)
        {
            foreach (var spoolFile in spoolFiles)
            {
                var code = spoolFile.GenerateCode();
                if (_progressingDict.TryGetValue(code, out SpoolFile processFile))
                {
                    _pendingQueue.Enqueue(processFile);
                    _progressingDict.TryRemove(code, out _);
                }
                else
                {
                    _logger.LogDebug("Return file not exist, the file may be released。FilePool :'{0}',Train:'{1}',Path:'{2}'.", spoolFile.FilePool, spoolFile.TrainIndex, spoolFile.Path);
                }
            }
        }

        /// <summary>
        /// Release files
        /// </summary>
        /// <param name="spoolFiles"></param>
        public void ReleaseFiles(params SpoolFile[] spoolFiles)
        {
            try
            {
                foreach (var spoolFile in spoolFiles)
                {
                    if (_progressingDict.TryRemove(spoolFile.GenerateCode(), out SpoolFile deleteFile))
                    {
                        FilePathUtil.DeleteFileIfExists(deleteFile.Path);
                    }
                    else
                    {
                        _logger.LogDebug("Can't find release file in queue,file info:{0}", deleteFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Release file failed:{0}", ex.Message);
                throw ex;
            }
            finally
            {
                DeleteFromLocal();
            }
        }

        /// <summary>
        /// Change the train type
        /// </summary>
        /// <param name="type"></param>
        public void ChangeType(TrainType type)
        {
            var sourceType = TrainType;
            TrainType = type;

            //do some things
            if (OnTypeChange != null)
            {
                var info = BuildInfo();
                var args = new TrainTypeChangeEventArgs()
                {
                    SourceType = sourceType,
                    DestinationType = type,
                    Train = info,
                };
                OnTypeChange.Invoke(this, args);
            }

            //如果从未操作变成读,或者写
            if (sourceType == TrainType.Default && type != TrainType.Default)
            {
                LoadFiles();
            }
        }

        #region Private methods

        /// <summary>
        /// Whether the file can delete
        /// </summary>
        private bool CanDeleteFromLocal()
        {
            return IsPendingEmpty() && _progressingDict.Count == 0 && TrainType == TrainType.Read;
        }

        /// <summary>
        /// Delete file from local path
        /// </summary>
        private void DeleteFromLocal()
        {
            if (CanDeleteFromLocal())
            {
                //删除
                var info = BuildInfo();
                var args = new TrainDeleteEventArgs()
                {
                    Train = info
                };
                OnDelete?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Build train info
        /// </summary>
        /// <returns></returns>
        private TrainInfo BuildInfo()
        {
            var info = new TrainInfo()
            {
                FilePool = _configuration.Name,
                FilePoolPath = _configuration.Path,
                Index = Index,
                Name = Name,
                Path = Path,
                TrainType = TrainType
            };
            return info;
        }

        /// <summary>
        /// Load train files
        /// </summary>
        private void LoadFiles()
        {
            var directoryInfo = new DirectoryInfo(Path);
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                var spoolFile = new SpoolFile()
                {
                    FilePool = _configuration.Name,
                    TrainIndex = Index,
                    Path = file.FullName
                };
                _pendingQueue.Enqueue(spoolFile);
            }
        }

        /// <summary>
        /// Generate file path
        /// </summary>
        private string GenerateFilePath(string fileExt)
        {
            // D:\\pool1\\_000001_
            var fileName = $"{ObjectId.GenerateNewStringId()}{fileExt}";
            var path = System.IO.Path.Combine(_configuration.Path, $"{Name}", fileName);
            return path;
        }

        private async ValueTask WriteInternalAsync(Stream stream, string path)
        {
            using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            await stream.CopyToAsync(fs);
            //using FileStream fs = File.OpenWrite(path);
            //var buffers = new byte[_configuration.WriteBufferSize];
            //int r = stream.Read(buffers, 0, buffers.Length);
            //while (r > 0)
            //{
            //    fs.Write(buffers, 0, r);
            //    r = stream.Read(buffers, 0, buffers.Length);
            //}
        }
        #endregion


    }
}
