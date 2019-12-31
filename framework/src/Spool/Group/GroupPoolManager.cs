using Microsoft.Extensions.Logging;
using Spool.Dependency;
using Spool.Utility;
using System;
using System.Collections.Generic;
using System.IO;

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

        /// <summary>Find GroupPoolDescriptors from spool folder
        /// </summary>
        public List<GroupPoolDescriptor> FindGroupPools()
        {
            var groupPoolDescriptors = new List<GroupPoolDescriptor>();
            var spoolDirectoryInfo = new DirectoryInfo(_option.RootPath);
            var directoryInfos = spoolDirectoryInfo.GetDirectories();
            foreach (var directoryInfo in directoryInfos)
            {
                if (directoryInfo.Name != Consts.FILEPOOL_DATA_NAME)
                {
                    var groupPoolDescriptor = new GroupPoolDescriptor()
                    {
                        GroupName = directoryInfo.Name,
                        GroupPath = directoryInfo.FullName
                    };
                    groupPoolDescriptors.Add(groupPoolDescriptor);
                }
            }
            _logger.LogInformation("Find GroupPoolDescriptors from spool folder,{0}", string.Join(",", groupPoolDescriptors));

            return groupPoolDescriptors;
        }

        /// <summary>Create group pool by 'GroupPoolDescriptor'
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public GroupPool CreateGroupPool(GroupPoolDescriptor descriptor)
        {
            if (DirectoryHelper.CreateIfNotExists(descriptor.GroupPath))
            {
                _logger.LogInformation("Create groupPool directory,groupName '{0}',directory '{1}'", descriptor.GroupName, descriptor.GroupPath);
            }
            var groupPool = _provider.CreateInstance<GroupPool>(descriptor);
            return groupPool;
        }

    }
}
