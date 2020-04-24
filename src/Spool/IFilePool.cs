using System;
using System.IO;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>文件池
    /// </summary>
    public interface IFilePool
    {
        /// <summary>文件池的配置信息
        /// </summary>
        FilePoolOption Option { get; }

        /// <summary>是否正在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>归还文件事件
        /// </summary>
        event EventHandler<ReturnFileEventArgs> OnFileReturn;

        /// <summary>运行文件池
        /// </summary>
        void Start();

        /// <summary>关闭文件池
        /// </summary>
        void Shutdown();

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        Task<SpoolFile> WriteFileAsync(Stream stream, string fileExt);

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        SpoolFile WriteFile(Stream stream, string fileExt);

        /// <summary>获取指定数量的文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        SpoolFile[] GetFiles(int count = 1);

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        void ReturnFiles(params SpoolFile[] spoolFiles);

        /// <summary>释放文件
        /// </summary>
        void ReleaseFiles(params SpoolFile[] spoolFiles);
    }
}
