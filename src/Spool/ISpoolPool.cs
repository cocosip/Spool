using System;
using System.IO;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>
    /// </summary>
    public interface ISpoolPool
    {
        /// <summary>运行状态
        /// </summary>
        bool IsRunning { get; }

        /// <summary>配置信息
        /// </summary>
        SpoolOption Option { get; }

        /// <summary>写入文件事件
        /// </summary>
        event EventHandler<WriteFileEventArgs> OnFileWrite;

        /// <summary>取走文件事件
        /// </summary>
        event EventHandler<GetFileEventArgs> OnFileGet;

        /// <summary>删除文件事件
        /// </summary>
        event EventHandler<ReleaseFileEventArgs> OnFileRelease;

        /// <summary>归还文件事件
        /// </summary>
        event EventHandler<ReturnFileEventArgs> OnFileReturn;

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="poolName">组名</param>
        /// <returns></returns>
        Task<SpoolFile> WriteAsync(Stream stream, string fileExt, string poolName = "");

        /// <summary>写文件
        /// </summary>
        /// <param name="buffer">文件二进制流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="poolName">组名</param>
        /// <returns></returns>
        Task<SpoolFile> WriteAsync(byte[] buffer, string fileExt, string poolName = "");

        /// <summary>写文件
        /// </summary>
        /// <param name="filename">文件名(全路径)</param>
        /// <param name="poolName">组名</param>
        /// <returns></returns>
        Task<SpoolFile> WriteAsync(string filename, string poolName = "");

        /// <summary>获取文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="poolName">组名</param>
        /// <returns></returns>
        SpoolFile[] Get(int count, string poolName = "");

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        /// <param name="poolName">组名</param>
        void Return(string poolName = "", params SpoolFile[] spoolFiles);

        /// <summary>释放文件
        /// </summary>
        /// <param name="poolName">组名</param>
        /// <param name="spoolFiles">文件列表</param>

        void Release(string poolName = "", params SpoolFile[] spoolFiles);

        /// <summary>运行
        /// </summary>
        void Start();

        /// <summary>关闭
        /// </summary>
        void Shutdown();
    }
}
