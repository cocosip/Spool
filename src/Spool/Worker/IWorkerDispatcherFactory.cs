namespace Spool.Worker
{
    /// <summary>
    /// 调度器工厂
    /// </summary>
    public interface IWorkerDispatcherFactory
    {
        /// <summary>
        /// GetOrAdd worker dispatcher
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IWorkerDispatcher GetOrAdd(string name);
    }
}
