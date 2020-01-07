using System.Collections.Concurrent;

namespace Spool.Utility
{
    /// <summary>ConcurrentQueue extension methods
    /// </summary>
    public static class ConcurrentQueueExtensions
    {
        /// <summary>Clean
        /// </summary>
        public static void Clean<T>(ConcurrentQueue<T> queue)
        {
            while (!queue.IsEmpty)
            {
                queue.TryDequeue(out T t);
            }
        }
    }
}
