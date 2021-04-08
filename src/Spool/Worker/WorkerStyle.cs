namespace Spool.Worker
{
    /// <summary>
    /// 工作者类型
    /// </summary>
    public enum WorkerStyle
    {
        /// <summary>
        /// 还未做任何处理
        /// </summary>
        None = -1,

        /// <summary>
        /// 只读
        /// </summary>
        ReadOnly = 1,

        /// <summary>
        /// 只写
        /// </summary>
        WriteOnly = 2,

        /// <summary>
        /// 可读可写
        /// </summary>
        ReadWrite = 4

    }
}
