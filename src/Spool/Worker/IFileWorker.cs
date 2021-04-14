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
    }
}
