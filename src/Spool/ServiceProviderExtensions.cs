using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spool
{
    /// <summary>依赖注入扩展
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>Spool依赖注入
        /// </summary>
        public static IServiceProvider ConfigureSpool(this IServiceProvider provider, Action<SpoolOption> configure = null)
        {
            //设置全局的依赖注入
            var host = provider.GetService<ISpoolHost>();
            host.SetupDI(provider);

            var option = provider.GetService<SpoolOption>();
            configure?.Invoke(option);

            //运行SpoolPool
            var spoolPool = provider.GetService<SpoolPool>();
            spoolPool.Start();

            return provider;
        }

    }
}
