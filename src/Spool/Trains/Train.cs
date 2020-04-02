using Microsoft.Extensions.Logging;
using Spool.Utility;
using Spool.Writers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SystemPath = System.IO.Path;

namespace Spool.Trains
{
    /// <summary>序列
    /// </summary>
    public class Train
    {
        /// <summary>序列的索引
        /// </summary>
        public int Index { get; private set; }

        /// <summary>序列名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>序列的路径
        /// </summary>
        public string Path { get; private set; }

        /// <summary>序列类型
        /// </summary>
        public TrainType TrainType { get; set; }

        /// <summary>是否已经初始化
        /// </summary>
        public bool Initialized { get; private set; } = false;


        /// <summary>当前序列下文件的全部索引
        /// </summary>
        private readonly ConcurrentQueue<SpoolFile> _pendingQueue;

        /// <summary>进行中的序列下的文件操作
        /// </summary>
        private readonly ConcurrentDictionary<string, SpoolFile> _progressingDict;

        /// <summary>序列删除事件
        /// </summary>
        public event EventHandler<TrainDeleteEventArgs> OnDelete;

        /// <summary>序列类型转换事件
        /// </summary>
        public event EventHandler<TrainTypeChangeEventArgs> OnTypeChange;

        /// <summary>序列写满
        /// </summary>
        public event EventHandler<TrainWriteOverEventArgs> OnWriteOver;

        ///// <summary>序列标记为删除后,有归还操作
        ///// </summary>
        //public event EventHandler<TrainDeleteReturnFilesEventArgs> OnDeleteReturn;

        private readonly ILogger _logger;
        private readonly FilePoolOption _option;
        private readonly IdGenerator _idGenerator;
        private readonly IFileWriterManager _fileWriterManager;

        public Train(ILogger<Train> logger, FilePoolOption option, IdGenerator idGenerator, IFileWriterManager fileWriterManager, TrainOption trainOption)
        {
            _logger = logger;
            _option = option;
            _idGenerator = idGenerator;
            _fileWriterManager = fileWriterManager;

            Index = trainOption.Index;
            Name = TrainUtil.GenerateTrainName(Index);
            Path = TrainUtil.GenerateTrainPath(_option.Path, Name);
            TrainType = TrainType.Default;
            _pendingQueue = new ConcurrentQueue<SpoolFile>();
            _progressingDict = new ConcurrentDictionary<string, SpoolFile>();
        }




        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteFileAsync(Stream stream, string fileExt)
        {
            return await Task.Run(() =>
            {
                return WriteFile(stream, fileExt);
            });
        }

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public SpoolFile WriteFile(Stream stream, string fileExt)
        {
            var spoolFile = new SpoolFile()
            {
                FilePoolName = _option.Name,
                TrainIndex = Index
            };
            var fileWriter = _fileWriterManager.Get();
            try
            {

                var path = GenerateFilePath(fileExt);
                fileWriter.WriteFile(stream, path);
                spoolFile.Path = path;

                //写入队列
                _pendingQueue.Enqueue(spoolFile);

                //是否写满了
                if (_pendingQueue.Count > _option.TrainMaxFileCount)
                {
                    var info = BuildInfo();
                    var args = new TrainWriteOverEventArgs()
                    {
                        Info = info,
                    };
                    OnWriteOver.Invoke(this, args);
                }


                return spoolFile;
            }
            catch (Exception ex)
            {
                _logger.LogError("写入文件出错,当前文件扩展名:'{0}',异常信息:{1}.", fileExt, ex.Message);
                throw ex;
            }
            finally
            {
                _fileWriterManager.Return(fileWriter);
                //流释放
                stream?.Close();
                stream?.Dispose();
            }
        }


        /// <summary>获取指定数量的文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public SpoolFile[] GetFiles(int count = 1)
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
                _logger.LogError("从序列:'{0}' 中获取文件失败。异常信息:{1}", Index, ex.Message);
                //如果出现异常,则判断集合是否为空
                if (spoolFiles.Any())
                {
                    ReturnFiles(spoolFiles.ToArray());
                }
            }
            return spoolFiles.ToArray();
        }

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        public void ReturnFiles(params SpoolFile[] spoolFiles)
        {
            foreach (var spoolFile in spoolFiles)
            {
                var code = spoolFile.GenerateCode();
                if (_progressingDict.TryGetValue(code, out SpoolFile processFile))
                {
                    //文件存在,则移除,放回到原先的队列中
                    _pendingQueue.Enqueue(processFile);
                    _progressingDict.TryRemove(code, out _);
                }
                else
                {
                    _logger.LogDebug("归还数据文件不存在,组:'{0}',序列索引:'{1}',文件路径:'{2}'.", spoolFile.FilePoolName, spoolFile.TrainIndex, spoolFile.Path);
                }
            }
        }

        /// <summary>释放文件
        /// </summary>
        public void ReleaseFiles(params SpoolFile[] spoolFiles)
        {
            try
            {
                foreach (var spoolFile in spoolFiles)
                {
                    if (_progressingDict.TryRemove(spoolFile.GenerateCode(), out SpoolFile deleteFile))
                    {
                        FileHelper.DeleteIfExists(deleteFile.Path);
                    }
                    else
                    {
                        _logger.LogDebug("释放文件时,未在处理中的队列发现文件,文件信息:{0}", deleteFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("释放文件出错:{0}", ex.Message);
            }
            finally
            {
                RealDelete();
            }
        }

        /// <summary>序列信息
        /// </summary>
        public string Info()
        {
            return $"[文件池名:{_option.Name},文件池路径:{_option.Path},序列索引:{Index},序列名:{Name},序列路径:{Path}]";
        }

        /// <summary>初始化
        /// </summary>
        public void Initialize()
        {
            if (Initialized)
            {
                _logger.LogDebug("序列已经初始化,文件池名:'{0}',当前序列索引:'{1}'.", _option.Name, Index);
                return;
            }

            //创建序列文件夹
            if (DirectoryHelper.CreateIfNotExists(Path))
            {
                _logger.LogDebug("创建序列文件夹,文件池名:'{0}',文件池路径:'{1}',当前序列索引:'{2}',序列路径:'{3}'.", _option.Name, _option.Path, Index, Path);
            }
            Initialized = true;
        }

        /// <summary>能否释放
        /// </summary>
        public bool IsEmpty()
        {
            return _pendingQueue.IsEmpty;
        }

        /// <summary>能否删除
        /// </summary>
        public bool CanDelete()
        {
            return IsEmpty() && _progressingDict.Count == 0 && TrainType == TrainType.Read;
        }



        /// <summary>序列类型转换
        /// </summary>
        public void ChangeType(TrainType type)
        {
            var sourceType = this.TrainType;
            this.TrainType = type;

            //需要做一些事情
            if (OnTypeChange != null)
            {
                var info = BuildInfo();
                var args = new TrainTypeChangeEventArgs()
                {
                    SourceType = sourceType,
                    Info = info,
                };
                OnTypeChange.Invoke(this, args);
            }

            //如果从未操作变成读,或者写
            if (sourceType == TrainType.Default && type != TrainType.Default)
            {
                LoadFiles();
            }
        }

        /// <summary>真实的删除
        /// </summary>
        private void RealDelete()
        {
            if (CanDelete())
            {
                //删除
                var info = BuildInfo();
                var args = new TrainDeleteEventArgs()
                {
                    Info = info
                };
                OnDelete?.Invoke(this, args);
            }
        }

        /// <summary>家在当前序列的文件
        /// </summary>
        private void LoadFiles()
        {
            var directoryInfo = new DirectoryInfo(Path);
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                var spoolFile = new SpoolFile()
                {
                    FilePoolName = _option.Name,
                    TrainIndex = Index,
                    Path = file.FullName
                };
                _pendingQueue.Enqueue(spoolFile);
            }
        }

        /// <summary>根据序列信息,文件池配置信息获取序列基本信息
        /// </summary>
        private TrainInfo BuildInfo()
        {
            var info = new TrainInfo()
            {
                FilePoolName = _option.Name,
                FilePoolPath = _option.Path,
                Index = Index,
                Name = Name,
                Path = Path,
                TrainType = TrainType
            };
            return info;
        }


        /// <summary>根据文件扩展名生成存储路径
        /// </summary>
        private string GenerateFilePath(string fileExt)
        {
            //组/索引/
            var fileId = $"{_idGenerator.GenerateIdAsString()}{fileExt}";
            var path = SystemPath.Combine(_option.Path, $"{Name}", fileId);
            return path;
        }



    }

}
