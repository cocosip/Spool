using Microsoft.Extensions.Logging;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Spool.Worker
{
    /// <summary>
    /// 文件读写工作者,一个工作者保持一个目录
    /// </summary>
    public class DefaultFileWorker : IFileWorker
    {
        private WorkerStyle _style = WorkerStyle.None;
        private WorkerState _state = WorkerState.Pending;

        /// <summary>
        /// 工作者类型
        /// </summary>
        public WorkerStyle Style => _style;

        /// <summary>
        /// 工作者状态
        /// </summary>
        public WorkerState State => _state;

        private readonly ILogger _logger;

        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get { return _index; } }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 文件池路径
        /// </summary>
        private readonly string _filePoolPath;

        /// <summary>
        /// 文件池名称
        /// </summary>
        private readonly string _filePoolName;

        /// <summary>
        /// 每次写入的文件是字节大小
        /// </summary>
        private readonly int _writeBufferSize;

        /// <summary>
        /// 最大的文件数量,写入超过该值,则不会再写入
        /// </summary>
        private readonly int _maxFileCount;

        /// <summary>
        /// 当前序号
        /// </summary>
        private readonly int _index;

        private readonly ConcurrentQueue<SpoolFile> _pendingQueue;
        private readonly ConcurrentDictionary<string, SpoolFile> _progressingDict;
        /// <summary>
        /// Ctor
        /// </summary>
        public DefaultFileWorker(ILogger<DefaultFileWorker> logger, string filePoolName, string filePoolPath, int writeBufferSize, int maxFileCount, int index)
        {
            _logger = logger;

            _filePoolName = filePoolName;
            _filePoolPath = filePoolPath;
            _writeBufferSize = writeBufferSize;
            _maxFileCount = maxFileCount;
            _index = index;
            Name = FileWorkerUtil.GenerateName(_index);
            Path = FileWorkerUtil.GeneratePath(_filePoolPath, Name);

            _pendingQueue = new ConcurrentQueue<SpoolFile>();
            _progressingDict = new ConcurrentDictionary<string, SpoolFile>();
        }

        /// <summary>
        /// 当前FileWorker信息
        /// </summary>
        /// <returns></returns>
        public string Info()
        {
            return $"[FilePool:{_filePoolName},FilePool path:{_filePoolPath},Index:{Index},Train name:{Name},Train path:{Path}]";
        }

        /// <summary>
        /// 写入Spool
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public async Task<SpoolFile> WriteFileAsync(Stream stream, string ext)
        {
            var spoolFile = new SpoolFile(_filePoolName, _index);
            try
            {
                var path = GenerateFilePath(ext);
                spoolFile.Path = path;
                //Write file
                await WriteInternalAsync(stream, path);

                //Write queue
                _pendingQueue.Enqueue(spoolFile);

                //是否写满了(需要按照待处理的文件数量+处理中的数量进行计算,避免当关闭自动归还功能时,磁盘下的文件还有大量的堆积)
                if (_pendingQueue.Count + _progressingDict.Count > _maxFileCount)
                {
                    _logger.LogInformation("FileWorker 写入文件已满,{0}.", Info());
                    //TODO 写满时的操作
                }

                return spoolFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入FileWorker出现了一些错误,异常信息:{0}.", ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 从列表中获取一个文件
        /// </summary>
        /// <returns></returns>
        public SpoolFile Get()
        {
            if (!_pendingQueue.TryDequeue(out SpoolFile spoolFile))
            {
                _logger.LogDebug("无法从FilePool:{0},序列:{1}中获取SpoolFile.", _filePoolName, _index);
            }
            return spoolFile;
        }

        /// <summary>
        /// 归还一个文件
        /// </summary>
        /// <param name="spoolFile"></param>
        public void ReturnFile(SpoolFile spoolFile)
        {
            var code = spoolFile.GenerateCode();
            if (_progressingDict.TryGetValue(code, out SpoolFile processFile))
            {
                _pendingQueue.Enqueue(processFile);
                _progressingDict.TryRemove(code, out _);
            }
            else
            {
                _logger.LogDebug("归还文件 {0} 失败,当前文件可能已经被释放.", spoolFile.FilePool, spoolFile.Index, spoolFile.Path);
            }
        }


        /// <summary>
        /// 生成文件存储的路径地址
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        private string GenerateFilePath(string ext)
        {
            var fileName = $"{Guid.NewGuid()}{ext}";
            var path = System.IO.Path.Combine(_filePoolPath, $"{Name}", fileName);
            return path;
        }

        /// <summary>
        /// 将流写入到文件
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task WriteInternalAsync(Stream stream, string path)
        {
            using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            await stream.CopyToAsync(fs);
        }

    }
}
