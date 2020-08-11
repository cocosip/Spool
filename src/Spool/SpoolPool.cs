using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>全局文件池
    /// </summary>
    public class SpoolPool : ISpoolPool
    {

        /// <summary>写入文件事件
        /// </summary>
        public event EventHandler<WriteFileEventArgs> OnFileWrite;

        /// <summary>取走文件事件
        /// </summary>
        public event EventHandler<GetFileEventArgs> OnFileGet;

        /// <summary>删除文件事件
        /// </summary>
        public event EventHandler<ReleaseFileEventArgs> OnFileRelease;

        /// <summary>归还文件事件
        /// </summary>
        public event EventHandler<ReturnFileEventArgs> OnFileReturn;


        private readonly ILogger _logger;
        private readonly SpoolOption _option;
        private readonly IFilePoolDescriptorSelector _filePoolDescriptorSelector;
        private readonly IFilePoolFactory _filePoolFactory;

        private object SyncObject = new object();

        /// <summary>文件池集合
        /// </summary>
        private readonly ConcurrentDictionary<string, IFilePool> _filePoolDict;

        /// <summary>ctor
        /// </summary>
        public SpoolPool(ILogger<SpoolPool> logger, IOptions<SpoolOption> options, IFilePoolDescriptorSelector filePoolDescriptorSelector, IFilePoolFactory filePoolFactory)
        {
            _logger = logger;
            _option = options.Value;
            _filePoolDescriptorSelector = filePoolDescriptorSelector;
            _filePoolFactory = filePoolFactory;

            _filePoolDict = new ConcurrentDictionary<string, IFilePool>();
        }

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="poolName">组名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteAsync(Stream stream, string fileExt, string poolName = "")
        {
            //获取文件池
            var filePool = GetFilePool(poolName);
            var spoolFile = await filePool.WriteFileAsync(stream, fileExt);

            OnFileWrite?.Invoke(this, new WriteFileEventArgs()
            {
                FilePoolName = filePool.Option.Name,
                SpoolFile = spoolFile
            });
            return spoolFile;
        }

        /// <summary>写文件
        /// </summary>
        /// <param name="buffer">文件二进制流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="poolName">组名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteAsync(byte[] buffer, string fileExt, string poolName = "")
        {
            using (var ms = new MemoryStream(buffer))
            {
                return await WriteAsync(ms, fileExt, poolName);
            }
        }


        /// <summary>写文件
        /// </summary>
        /// <param name="filename">文件名(全路径)</param>
        /// <param name="poolName">组名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteAsync(string filename, string poolName = "")
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var fileExt = FilePathUtil.GetPathExtension(filename);
                return await WriteAsync(fs, fileExt, poolName);
            }
        }

        /// <summary>获取文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="poolName">组名</param>
        /// <returns></returns>
        public SpoolFile[] Get(int count, string poolName = "")
        {
            var filePool = GetFilePool(poolName);
            var spoolFiles = filePool.GetFiles(count);
            var copySpoolFiles = spoolFiles.Select(x => x.Clone()).ToArray();
            OnFileGet?.Invoke(this, new GetFileEventArgs()
            {
                FilePoolName = filePool.Option.Name,
                SpoolFiles = copySpoolFiles,
                GetFileCount = count
            });
            return spoolFiles;
        }

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        /// <param name="poolName">组名</param>
        public void Return(string poolName = "", params SpoolFile[] spoolFiles)
        {
            var filePool = GetFilePool(poolName);
            filePool.ReturnFiles(spoolFiles);
        }

        /// <summary>释放文件
        /// </summary>
        /// <param name="poolName">组名</param>
        /// <param name="spoolFiles">文件列表</param>

        public void Release(string poolName = "", params SpoolFile[] spoolFiles)
        {
            var filePool = GetFilePool(poolName);
            filePool.ReleaseFiles(spoolFiles);
            OnFileRelease?.Invoke(this, new ReleaseFileEventArgs()
            {
                FilePoolName = filePool.Option.Name,
                SpoolFiles = spoolFiles
            });
        }

        /// <summary>获取文件数量
        /// </summary>
        public int GetPendingCount(string poolName = "")
        {
            var filePool = GetFilePool(poolName);
            return filePool.GetPendingCount();
        }

        /// <summary>获取取走的数量
        /// </summary>
        public int GetProcessingCount(string poolName = "")
        {
            var filePool = GetFilePool(poolName);
            return filePool.GetProcessingCount();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            foreach (var filePool in _filePoolDict.Values)
            {
                if (OnFileReturn != null)
                {
                    filePool.OnFileReturn += FilePool_OnFileReturn;
                }

                filePool.Shutdown();
            }
        }

        /// <summary>
        /// 文件归还事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilePool_OnFileReturn(object sender, ReturnFileEventArgs e)
        {
            OnFileReturn?.Invoke(this, e);
        }

        /// <summary>根据组名获取文件池
        /// </summary>
        private IFilePool GetFilePool(string poolName)
        {
            if (string.IsNullOrWhiteSpace(poolName))
            {
                poolName = _option.DefaultPool;
            }

            if (!_filePoolDict.TryGetValue(poolName, out IFilePool filePool))
            {

                lock (SyncObject)
                {
                    if (!_filePoolDict.TryGetValue(poolName, out filePool))
                    {
                        var filePoolDescriptor = _filePoolDescriptorSelector.GetDescriptor(poolName);
                        filePool = _filePoolFactory.CreateFilePool(filePoolDescriptor);

                        //判断当前是否有绑定归还事件
                        if (OnFileReturn != null)
                        {
                            filePool.OnFileReturn += FilePool_OnFileReturn;
                        }

                        if (_filePoolDict.TryAdd(poolName, filePool))
                        {
                            filePool.Start();
                        }
                        else
                        {
                            _logger.LogWarning("添加新建的文件池:{0}失败!", poolName);
                        }
                    }
                }
            }

            return filePool;
        }




    }
}
