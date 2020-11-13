using System.Collections.Generic;

namespace Spool
{
    /// <summary>
    /// File pool factory
    /// </summary>
    public interface IFilePoolFactory
    {
        /// <summary>
        /// Get or create file pool by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IFilePool GetOrCreate(string name);

        /// <summary>
        /// Get all file pools
        /// </summary>
        /// <returns></returns>
        List<IFilePool> GetAllFilePools();
    }
}
