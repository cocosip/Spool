namespace Spool.Workers
{
    public interface IWorkerFactory
    {
        /// <summary>
        /// Create worker by number
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        IWorker CreateWorker(FilePoolConfiguration configuration, int number);
    }
}