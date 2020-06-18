using System.IO;
using System.Threading.Tasks;

namespace Spool.Writers
{
    /// <summary>文件写入器
    /// </summary>
    public interface IFileWriter
    {
        /// <summary>写入器Id
        /// </summary>
        string Id { get; }

        /// <summary>写入文件
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="targetPath">存储路径</param>
        /// <returns></returns>
        Task WriteFileAsync(Stream stream, string targetPath);

        /// <summary>写入文件
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="targetPath">存储路径</param>
        /// <returns></returns>
        void WriteFile(Stream stream, string targetPath);
    }
}
