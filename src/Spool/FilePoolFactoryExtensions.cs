namespace Spool
{
    /// <summary>
    /// File pool factory extensions
    /// </summary>
    public static class FilePoolFactoryExtensions
    {
        /// <summary>
        /// Get or create file pool with generic type
        /// </summary>
        /// <typeparam name="TFilePool"></typeparam>
        /// <param name="filePoolFactory"></param>
        /// <returns></returns>
        public static IFilePool GetOrCreate<TFilePool>(this IFilePoolFactory filePoolFactory)
        {
            return filePoolFactory.GetOrCreate(
                FilePoolNameAttribute.GetFilePoolName<TFilePool>()
            );
        }
    }
}
