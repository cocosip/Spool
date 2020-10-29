namespace Spool
{
    public interface IFilePoolConfigurationSelector
    {
        /// <summary>
        /// 根据名称获取文件池配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        FilePoolConfiguration Get(string name);
    }
}
