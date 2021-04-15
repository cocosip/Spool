namespace Spool.Worker
{
    /// <summary>
    /// Worker 调度器
    /// </summary>
    public interface IWorkerDispatcher
    {
        /// <summary>
        /// 文件池名称
        /// </summary>
        string Name { get; }
    }
}
