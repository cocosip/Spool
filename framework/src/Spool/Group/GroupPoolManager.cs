using Microsoft.Extensions.Logging;
using Spool.Dependency;
using Spool.Utility;
using System;

namespace Spool.Group
{
    public class GroupPoolManager : IGroupPoolManager
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _provider;
        private readonly SpoolOption _option;

        public GroupPoolManager(ILogger<GroupPoolManager> logger, IServiceProvider provider, SpoolOption option)
        {
            _logger = logger;
            _provider = provider;
            _option = option;
        }


        /// <summary>Create group pool by 'GroupPoolDescriptor'
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public GroupPool CreateGroupPool(SpoolGroupDescriptor descriptor)
        {
            if (DirectoryHelper.CreateIfNotExists(descriptor.Path))
            {
                _logger.LogInformation("Create groupPool directory,groupName '{0}',directory '{1}'", descriptor.Name, descriptor.Path);
            }
            var groupPool = _provider.CreateInstance<GroupPool>(descriptor);
            return groupPool;
        }

    }
}
