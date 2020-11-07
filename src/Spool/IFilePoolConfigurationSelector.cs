namespace Spool
{
    /// <summary>
    /// File pool configuration selector
    /// </summary>
    public interface IFilePoolConfigurationSelector
    {
        /// <summary>
        /// Get file pool configuration by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        FilePoolConfiguration Get(string name);
    }
}
