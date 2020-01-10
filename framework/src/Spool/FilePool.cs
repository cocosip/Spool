using Microsoft.Extensions.Logging;
using Spool.Group;
using Spool.Utility;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;

namespace Spool
{
    /// <summary>FilePool,get and store files.
    /// </summary>
    public class FilePool
    {
        public bool IsRunning { get { return _isRunning == 1; } }
        public string Id { get; }
        private int _isRunning = 0;
        public string DataPath { get; private set; }
        private readonly ConcurrentDictionary<string, GroupPool> _groupPoolDict;

        private readonly ILogger _logger;
        private readonly SpoolOption _option;
        private readonly IGroupPoolManager _groupPoolManager;

        /// <summary>Ctor
        /// </summary>
        public FilePool(ILogger<FilePool> logger, SpoolOption option, IGroupPoolManager groupPoolManager)
        {
            _logger = logger;
            _option = option;
            _groupPoolManager = groupPoolManager;

            _groupPoolDict = new ConcurrentDictionary<string, GroupPool>();
            Id = Guid.NewGuid().ToString("N");
            DataPath = Path.Combine(_option.RootPath, Consts.FILEPOOL_DATA_NAME);
        }


        /// <summary>Start the filePool 
        /// </summary>
        public void Start()
        {
            if (_isRunning == 1)
            {
                _logger.LogInformation("FilePool '{0}' is in running ,don't run it again!", Id);
                return;
            }
            Initialize();

            Interlocked.Exchange(ref _isRunning, 1);
        }

        /// <summary>Shutdown the filePool
        /// </summary>
        public void Shutdown()
        {
            Interlocked.Exchange(ref _isRunning, 0);
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

            //Data path
            if (DirectoryHelper.CreateIfNotExists(DataPath))
            {
                _logger.LogInformation("FilePool '{0}', create fle pool data directory '{1}'.", Id, DataPath);
            }

            if (DirectoryHelper.CreateIfNotExists(_option.WatcherPath))
            {
                _logger.LogInformation("FilePool '{1}', create watcher directory '{1}'.", Id, _option.WatcherPath);
            }

            //GroupPool
            var groupPoolDescriptors = _groupPoolManager.FindGroupPools();
            if (!groupPoolDescriptors.Any())
            {
                groupPoolDescriptors.Add(new GroupPoolDescriptor()
                {
                    GroupName = _option.DefaultGroup,
                    GroupPath = GenerateGroupPath(_option.DefaultGroup)
                });
            }

            foreach (var groupPoolDescriptor in groupPoolDescriptors)
            {
                var groupPool = _groupPoolManager.CreateGroupPool(groupPoolDescriptor);
                groupPool.Initialize();
                if (!_groupPoolDict.TryAdd(groupPool.GroupName, groupPool))
                {
                    _logger.LogError("Add 'GroupPool' to dict fail ! GroupName is '{0}'", groupPoolDescriptor.GroupName);
                }
            }

        }

        private string GenerateGroupPath(string groupName)
        {
            return Path.Combine(_option.RootPath, groupName);
        }


    }
}
