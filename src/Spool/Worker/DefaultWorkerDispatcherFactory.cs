using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Spool.Worker
{
    /// <summary>
    /// 调度器工厂
    /// </summary>
    public class DefaultWorkerDispatcherFactory : IWorkerDispatcherFactory
    {
        private readonly object _sync = new();
        private readonly ConcurrentDictionary<string, IWorkerDispatcher> _dispatcherDict;
        private readonly ILogger _logger;

        /// <summary>
        /// Ctor
        /// </summary>
        public DefaultWorkerDispatcherFactory(ILogger<DefaultWorkerDispatcherFactory> logger)
        {
            _logger = logger;
            _dispatcherDict = new ConcurrentDictionary<string, IWorkerDispatcher>();
        }

        /// <summary>
        /// GetOrAdd worker dispatcher
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IWorkerDispatcher GetOrAdd(string name)
        {
            if (!_dispatcherDict.TryGetValue(name, out IWorkerDispatcher dispatcher))
            {
                lock (_sync)
                {
                    if (!_dispatcherDict.TryGetValue(name, out dispatcher))
                    {
                        dispatcher = CreateDispatcher();
                        if (!_dispatcherDict.TryAdd(name, dispatcher))
                        {
                            _logger.LogWarning("Add WorkDispatcher to dict failed.Name :'{0}'.", name);
                        }
                    }
                }
            }

            if (dispatcher == null)
            {
                throw new Exception($"Could not find any work dispatcher by name '{name}'.");
            }
            return dispatcher;
        }

        private IWorkerDispatcher CreateDispatcher()
        {
            //var dispatcher = new DefaultWorkerDispatcher();
            //return dispatcher;
            return default;
        }


    }
}
