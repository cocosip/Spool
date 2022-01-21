using System;
using Microsoft.Extensions.DependencyInjection;

namespace Spool.Workers
{
    public class WorkerFactory : IWorkerFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public WorkerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Create worker by number
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public IWorker CreateWorker(FilePoolConfiguration configuration, int number)
        {
            var worker = ActivatorUtilities.CreateInstance<FileWorker>(_serviceProvider, configuration, number);
            worker.Setup();
            return worker;
        }

    }
}