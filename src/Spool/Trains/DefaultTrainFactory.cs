using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Spool.Trains
{
    /// <summary>
    /// Default train factory
    /// </summary>
    public class DefaultTrainFactory : ITrainFactory
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;


        /// <summary>
        /// Ctor
        /// </summary>
        public DefaultTrainFactory(ILogger<DefaultTrainFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Create a new train
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public ITrain Create(FilePoolConfiguration configuration, int index)
        {
            ITrain train = new Train(_serviceProvider.GetService<ILogger<Train>>(), configuration, index);
            _logger.LogDebug("Create new train '{0}' in file pool '{1}' .", index, configuration.Name);
            return train;
        }


    }
}
