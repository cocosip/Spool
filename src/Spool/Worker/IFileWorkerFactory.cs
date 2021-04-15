using System.Collections.Generic;

namespace Spool.Worker
{

    /// <summary>
    /// FileWorker工厂
    /// </summary>
    public interface IFileWorkerFactory
    {
        /// <summary>
        /// 从文件池的配置文件中,加载本地FileWorkers
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        List<IFileWorker> LoadFileWorkers(FilePoolConfiguration configuration);

        /// <summary>
        /// 根据参数创建FileWorker
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        IFileWorker CreateFileWorker(FilePoolConfiguration configuration, int index);
    }
}
