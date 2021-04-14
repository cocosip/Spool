using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Spool.Worker
{
    /// <summary>
    /// Worker 调度器
    /// </summary>
    public class DefaultWorkerDispatcher : IWorkerDispatcher
    {
        private readonly ILogger _logger;
        private FilePoolConfiguration _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public DefaultWorkerDispatcher(
            ILogger<DefaultWorkerDispatcher> logger,
            FilePoolConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }




    }
}
