namespace Spool.Writers
{
    /// <summary>FileWriter创建器
    /// </summary>
    public interface IFileWriterBuilder
    {
        /// <summary>创建文件写入器
        /// </summary>
        IFileWriter BuildWriter(FilePoolOption option);
    }
}
