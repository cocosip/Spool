namespace Spool
{
    public static class FilePoolFactoryExtensions
    {
        /// <summary>
        /// 根据库泛型获取或者创建文件池
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
