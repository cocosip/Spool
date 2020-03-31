using Microsoft.Extensions.Logging;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>全局文件池
    /// </summary>
    public class SpoolPool
    {
        /// <summary>运行状态
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        private readonly ILogger _logger;
        private readonly SpoolOption _option;
        private readonly IFilePoolFactory _filePoolFactory;

        /// <summary>文件池集合
        /// </summary>
        private readonly ConcurrentDictionary<string, FilePool> _filePoolDict;

        /// <summary>ctor
        /// </summary>
        public SpoolPool(ILogger<SpoolPool> logger, SpoolOption option, IFilePoolFactory filePoolFactory)
        {
            _logger = logger;
            _option = option;
            _filePoolFactory = filePoolFactory;

            _filePoolDict = new ConcurrentDictionary<string, FilePool>();
        }

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteAsync(Stream stream, string fileExt, string groupName = "")
        {
            //获取文件池
            var filePool = GetFilePool(groupName);
            return await filePool.WriteFile(stream, fileExt);
        }

        /// <summary>写文件
        /// </summary>
        /// <param name="buffer">文件二进制流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteAsync(byte[] buffer, string fileExt, string groupName = "")
        {
            using (var ms = new MemoryStream(buffer))
            {
                return await WriteAsync(ms, fileExt, groupName);
            }
        }


        /// <summary>写文件
        /// </summary>
        /// <param name="filename">文件名(全路径)</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteAsync(string filename, string groupName = "")
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var fileExt = PathUtil.GetPathExtension(filename);
                return await WriteAsync(fs, fileExt, groupName);
            }
        }

        /// <summary>获取文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        public List<SpoolFile> Get(int count, string groupName = "")
        {
            var filePool = GetFilePool(groupName);
            return filePool.GetFiles(count);
        }

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        /// <param name="groupName">组名</param>
        public void Return(List<SpoolFile> spoolFiles, string groupName = "")
        {
            var filePool = GetFilePool(groupName);
            filePool.ReturnFiles(spoolFiles);
        }

        /// <summary>释放文件
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        /// <param name="groupName">组名</param>
        public void Release(List<SpoolFile> spoolFiles, string groupName = "")
        {
            var filePool = GetFilePool(groupName);
            filePool.ReleaseFiles(spoolFiles);
        }

        /// <summary>运行
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                _logger.LogInformation("SpoolPool已经正在运行,请不要重复启动!");
                return;
            }

            foreach (var descriptor in _option.FilePools)
            {
                var filePool = _filePoolFactory.CreateFilePool(descriptor);
                if (!_filePoolDict.TryAdd(descriptor.Name, filePool))
                {
                    _logger.LogWarning("添加文件池FilePool到集合失败,文件池名:{0},文件池路径:{1}", descriptor.Name, descriptor.Path);
                }
                filePool.Start();
            }

            IsRunning = true;
        }

        /// <summary>关闭
        /// </summary>
        public void Shutdown()
        {
            if (!IsRunning)
            {
                _logger.LogInformation("SpoolPool已经关闭,请不要重复关闭!");
                return;
            }
            IsRunning = false;
        }


        /// <summary>根据组名获取文件池
        /// </summary>
        private FilePool GetFilePool(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                groupName = _option.DefaultPool;
            }
            if (!_filePoolDict.TryGetValue(groupName, out FilePool filePool))
            {
                throw new ArgumentException($"未找到名为'{groupName}' 的文件池,请检查配置.");
            }
            return filePool;
        }



        /// <summary>关闭文件池
        /// </summary>
        private void ShutdownFilePools()
        {

        }

    }
}
