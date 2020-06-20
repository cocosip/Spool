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
        /// <summary>运行状态
        /// </summary>
        public bool IsRunning { get { return _isRunning == 1; } }

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

        /// <summary>配置信息
        /// </summary>
        public SpoolOption Option { get; private set; }

        private readonly ILogger _logger;
        private readonly IFilePoolFactory _filePoolFactory;

        private int _isRunning = 0;

        /// <summary>文件池集合
        /// </summary>
        private readonly ConcurrentDictionary<string, IFilePool> _filePoolDict;

        /// <summary>ctor
        /// </summary>
        public SpoolPool(ILogger<SpoolPool> logger, IOptions<SpoolOption> option, IFilePoolFactory filePoolFactory)
        {
            _logger = logger;
            Option = option.Value;
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

            OnFileReturn?.Invoke(this, new ReturnFileEventArgs()
            {
                FilePoolName = filePool.Option.Name,
                SpoolFiles = spoolFiles
            });
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

        /// <summary>运行
        /// </summary>
        public void Start()
        {
            if (_isRunning == 1)
            {
                _logger.LogInformation("SpoolPool已经正在运行,请不要重复启动!");
                return;
            }

            //判断是否有文件池
            if (!Option.FilePools.Any())
            {
                throw new ArgumentException("不存在任何文件池!");
            }
            //设置默认文件池名称
            if (!string.IsNullOrWhiteSpace(Option.DefaultPool))
            {
                if (!Option.FilePools.Any(x => string.Equals(x.Name, Option.DefaultPool, StringComparison.OrdinalIgnoreCase)))
                {
                    Option.DefaultPool = Option.FilePools.FirstOrDefault()?.Name;
                }
            }

            foreach (var descriptor in Option.FilePools)
            {
                var filePool = _filePoolFactory.CreateFilePool(descriptor);
                if (!_filePoolDict.TryAdd(descriptor.Name, filePool))
                {
                    _logger.LogWarning("添加文件池FilePool到集合失败,文件池名:{0},文件池路径:{1}", descriptor.Name, descriptor.Path);
                }
                //判断当前是否有绑定归还事件
                if (OnFileReturn != null)
                {
                    filePool.OnFileReturn += (o, e) =>
                    {
                        OnFileReturn?.Invoke(this, new ReturnFileEventArgs()
                        {
                            FilePoolName = e.FilePoolName,
                            SpoolFiles = e.SpoolFiles
                        });
                    };
                }
                filePool.Start();
            }

            Interlocked.Exchange(ref _isRunning, 1);
        }

        /// <summary>关闭
        /// </summary>
        public void Shutdown()
        {
            if (_isRunning == 0)
            {
                _logger.LogInformation("SpoolPool已经关闭,请不要重复关闭!");
                return;
            }

            Interlocked.Exchange(ref _isRunning, 0);
        }

        /// <summary>根据组名获取文件池
        /// </summary>
        private IFilePool GetFilePool(string poolName)
        {
            if (!string.IsNullOrWhiteSpace(poolName))
            {
                poolName = Option.DefaultPool;
            }

            if (!_filePoolDict.TryGetValue(poolName, out IFilePool filePool))
            {
                throw new ArgumentException($"未找到名为'{poolName}' 的文件池,请检查配置.");
            }
            return filePool;
        }

    }
}
