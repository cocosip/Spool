using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spool.Trains;
using System;
using System.Collections.Concurrent;

namespace Spool
{
    /// <summary>
    /// Default file pool factory
    /// </summary>
    public class DefaultFilePoolFactory : IFilePoolFactory
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFilePoolConfigurationSelector _configurationSelector;

        private readonly object _sync = new object();
        private readonly ConcurrentDictionary<string, IFilePool> _filePools;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="configurationSelector"></param>
        public DefaultFilePoolFactory(ILogger<DefaultFilePoolFactory> logger, IServiceProvider serviceProvider, IFilePoolConfigurationSelector configurationSelector)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configurationSelector = configurationSelector;
            _filePools = new ConcurrentDictionary<string, IFilePool>();
        }

        /// <summary>
        /// Get or create file pool by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IFilePool GetOrCreate(string name)
        {
            if (!_filePools.TryGetValue(name, out IFilePool filePool))
            {
                lock (_sync)
                {
                    if (!_filePools.TryGetValue(name, out filePool))
                    {
                        filePool = BuildFilePool(name);
                        filePool.Setup();

                        //
                        if (!_filePools.TryAdd(name, filePool))
                        {
                            _logger.LogWarning("Could not add file pool '{0}' to dict.", name);
                        }
                    }
                }
            }

            return filePool;
        }


        /// <summary>
        /// Create a new file pool
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private IFilePool BuildFilePool(string name)
        {
            var configuration = _configurationSelector.Get(name);
            if (configuration == null)
            {
                throw new ArgumentNullException($"Could not find any configuration for file pool '{name}', check your configurations.");
            }
            var logger = _serviceProvider.GetService<ILogger<FilePool>>();
            var trainFactory = _serviceProvider.GetService<ITrainFactory>();
            IFilePool filePool = new FilePool(logger, configuration, trainFactory);
            _logger.LogDebug("Create a new file pool by name '{0}'", name);
            return filePool;
        }

    }
}
