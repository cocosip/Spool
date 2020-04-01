using System;

namespace Spool
{

    /// <summary>Spool全局依赖注入信息
    /// </summary>
    public class SpoolHost : ISpoolHost
    {
        /// <summary>全局的依赖注入
        /// </summary>
        public IServiceProvider Provider { get; private set; }

        /// <summary>设置全局的DI
        /// </summary>
        public ISpoolHost SetupDI(IServiceProvider provider)
        {
            Provider = provider;
            return this;
        }
    }
}
