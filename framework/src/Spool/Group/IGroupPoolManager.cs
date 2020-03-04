namespace Spool.Group
{
    public interface IGroupPoolManager
    {
        /// <summary>Create group pool by 'GroupPoolDescriptor'
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        GroupPool CreateGroupPool(SpoolGroupDescriptor descriptor);
    }
}
