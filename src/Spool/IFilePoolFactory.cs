namespace Spool
{
    /// <summary>文件池管理
    /// </summary>
    public interface IFilePoolFactory
    {
        /// <summary>根据文件池描述生成文件池选项
        /// </summary>
        FilePoolOption CreateOption(FilePoolDescriptor descriptor);

        /// <summary>创建文件池
        /// </summary>
        IFilePool CreateFilePool(FilePoolDescriptor descriptor);

        ///// <summary>根据FilePoolDescriptor 设置FilePoolOption的值
        ///// </summary>
        //void SetScopeOption(FilePoolOption scopeOption, FilePoolDescriptor descriptor);

        /// <summary>给FilePoolOption赋值
        /// </summary>
        /// <param name="scopeOption">scope生命周期配置信息</param>
        /// <param name="option">当前文件池配置信息</param>
        void SetScopeOption(FilePoolOption scopeOption, FilePoolOption option);
    }
}
