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
        public static IServiceProvider UseSpool(this IServiceProvider provider, Action<SpoolOption> configure = null)
        {
            var option = provider.GetService<SpoolOption>();
            configure?.Invoke(option);

            //运行SpoolPool
            var spoolPool = provider.GetService<SpoolPool>();
            spoolPool.Start();

            return provider;
        }

        /// <summary>关闭Spool
        /// </summary>
        public static IServiceProvider ShutdownSpool(this IServiceProvider provider)
        {
            //运行SpoolPool
            var spoolPool = provider.GetService<SpoolPool>();
            spoolPool.Shutdown();

            return provider;
        }
    }
}
