using Microsoft.Extensions.Logging;
using Spool.Utility;
using Spool.Writers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        public string TrainPath { get; private set; }

        /// <summary>是否正在删除
        /// </summary>
        private bool _isDeleting = false;

        /// <summary>当前序列下文件的全部索引
        /// </summary>
        private readonly ConcurrentQueue<SpoolFile> _pendingQueue;

        /// <summary>进行中的序列下的文件操作
        /// </summary>

        private readonly ConcurrentDictionary<string, SpoolFile> _progressingDict;

        /// <summary>序列删除事件
        /// </summary>
        public event EventHandler<TrainDeleteEventArg> OnDelete;


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
            TrainPath = TrainUtil.GenerateTrainPath(_option.Path, Name);
            _pendingQueue = new ConcurrentQueue<SpoolFile>();
            _progressingDict = new ConcurrentDictionary<string, SpoolFile>();
        }

        /// <summary>序列信息
        /// </summary>
        public string Info()
        {
            return $"[文件池名:{_option.Name},文件池路径:{_option.Path},序列索引:{Index},序列名:{Name},序列路径:{TrainPath}]";
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
            return IsEmpty() && _progressingDict.Count == 0;
        }

        /// <summary>尝试删除当前序列
        /// </summary>
        public void MakeAsDelete()
        {
            _isDeleting = true;
        }

        /// <summary>真实的删除
        /// </summary>
        private void RealDelete()
        {
            if (_isDeleting && CanDelete())
            {
                //删除
                var arg = new TrainDeleteEventArg()
                {
                    GroupName = _option.Name,
                    GroupPath = _option.Path,
                    Index = Index
                };
                OnDelete?.Invoke(this, arg);
            }
        }


        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteFile(Stream stream, string fileExt)
        {
            var spoolFile = new SpoolFile()
            {
                GroupName = _option.Name,
                TrainIndex = Index
            };
            var fileWriter = _fileWriterManager.Get();
            try
            {

                var path = GenerateFilePath(fileExt);
                await fileWriter.WriteFileAsync(stream, path);
                spoolFile.Path = path;
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
                stream?.Dispose();
            }
        }

        /// <summary>获取指定数量的文件
        /// </summary>
        /// <param name="count">数量</param>
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
                _logger.LogError("从序列:'{0}' 中获取文件失败。异常信息:{1}", Index, ex.Message);
                //如果出现异常,则判断集合是否为空
                if (spoolFiles.Any())
                {
                    ReturnFiles(spoolFiles);
                }
            }
            return spoolFiles;
        }

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        public void ReturnFiles(List<SpoolFile> spoolFiles)
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
                    _logger.LogDebug("归还数据文件不存在,组:'{0}',序列索引:'{1}',文件路径:'{2}'.", spoolFile.GroupName, spoolFile.TrainIndex, spoolFile.Path);
                }
            }
        }

        /// <summary>释放文件
        /// </summary>
        public void ReleaseFiles(List<SpoolFile> spoolFiles)
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

        /// <summary>根据文件扩展名生成存储路径
        /// </summary>
        private string GenerateFilePath(string fileExt)
        {
            //组/索引/
            var fileId = $"{_idGenerator.GenerateIdAsString()}{fileExt}";
            var path = Path.Combine(_option.Path, $"{Name}", fileId);
            return path;
        }



    }

}
