using System;

namespace Spool
{
    /// <summary>Spool全局依赖注入信息
    /// </summary>
    public interface ISpoolHost
    {
        /// <summary>全局Provider
        /// </summary>
        IServiceProvider Provider { get; }

        /// <summary>设置全局的DI
        /// </summary>
        ISpoolHost SetupDI(IServiceProvider provider);
    }
}
