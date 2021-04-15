using System.IO;
using System.Threading.Tasks;

namespace Spool.Worker
{
    /// <summary>
    /// 文件读写工作者,一个工作者保持一个目录
    /// </summary>
    public interface IFileWorker
    {
        /// <summary>
        /// 当前序号
        /// </summary>
        int Index { get; }

        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 路径
        /// </summary>
        string Path { get; }

        /// <summary>
        /// 当前FileWorker信息
        /// </summary>
        /// <returns></returns>
        string Info();

        /// <summary>
        /// 写入Spool
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        Task<SpoolFile> WriteFileAsync(Stream stream, string ext);

        /// <summary>
        /// 从列表中获取一个文件
        /// </summary>
        /// <returns></returns>
        SpoolFile Get();

        /// <summary>
        /// 归还一个文件
        /// </summary>
        /// <param name="spoolFile"></param>
        void ReturnFile(SpoolFile spoolFile);

        /// <summary>
        /// 释放一个文件 
        /// </summary>
        /// <param name="spoolFile"></param>
        void ReleaseFile(SpoolFile spoolFile);
    }
}
