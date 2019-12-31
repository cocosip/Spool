using System.Collections.Generic;

namespace Spool.Group
{
    public interface IGroupPoolManager
    {
        /// <summary>Find GroupPoolDescriptors from spool folder
        /// </summary>
        List<GroupPoolDescriptor> FindGroupPools();

        /// <summary>Create group pool by 'GroupPoolDescriptor'
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        GroupPool CreateGroupPool(GroupPoolDescriptor descriptor);
    }
}
