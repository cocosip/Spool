using Microsoft.Extensions.Logging;
using Spool.Group;
using Spool.Utility;
using System;
using System.Collections.Concurrent;

namespace Spool
{
    /// <summary>FilePool,get and store files.
    /// </summary>
    public class FilePool
    {
        public bool IsRunning { get { return _isRunning; } }
        public string Id { get; }
        private bool _isRunning = false;
        private readonly ConcurrentDictionary<string, GroupPool> _groupPoolDict;
        
        private readonly ILogger _logger;
        private readonly SpoolOption _option;

        /// <summary>Ctor
        /// </summary>
        public FilePool(ILogger<FilePool> logger, SpoolOption option)
        {
            _logger = logger;
            _option = option;
            Id = Guid.NewGuid().ToString("N");
            _groupPoolDict = new ConcurrentDictionary<string, GroupPool>();
        }


        /// <summary>Start the filePool
        /// </summary>
        public void Start()
        {
            if (_isRunning)
            {
                _logger.LogInformation("FilePool '{0}' is in running ,don't run it again!", Id);
                return;
            }
            Initialize();
            _isRunning = true;
        }

        /// <summary>Shutdown the filePool
        /// </summary>
        public void Shutdown()
        {
            _isRunning = false;
        }

        /// <summary>Initialize
        /// </summary>
        private void Initialize()
        {
            _logger.LogInformation("Initialize filePool '{0}'.", Id);
            if (DirectoryHelper.CreateIfNotExists(_option.RootPath))
            {
                _logger.LogInformation("FilePool '{0}', create root directory '{1}'.", Id, _option.RootPath);
            }

            if (DirectoryHelper.CreateIfNotExists(_option.WatcherPath))
            {
                _logger.LogInformation("FilePool '{1}', create watcher directory '{1}'.", Id, _option.WatcherPath);
            }

        }


    }
}
