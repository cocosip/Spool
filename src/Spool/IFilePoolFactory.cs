namespace Spool
{
    public interface IFilePoolFactory
    {
        /// <summary>
        /// 获取或者创建一个文件池
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IFilePool GetOrCreate(string name);
    }
}
