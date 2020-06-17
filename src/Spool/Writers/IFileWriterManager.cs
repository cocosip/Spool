namespace Spool.Writers
{
    /// <summary>文件写入管理器
    /// </summary>
    public interface IFileWriterManager
    {

        /// <summary>获取一个文件写入器
        /// </summary>
        IFileWriter Get();

        /// <summary>归还一个文件写入器
        /// </summary>
        void Return(IFileWriter fileWriter);

    }
}
