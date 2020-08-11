namespace Spool
{
    /// <summary>
    /// 文件池配置信息筛选器
    /// </summary>
    public interface IFilePoolDescriptorSelector
    {
        /// <summary>
        /// Get FilePoolDescriptor by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        FilePoolDescriptor GetDescriptor(string name);
    }
}
