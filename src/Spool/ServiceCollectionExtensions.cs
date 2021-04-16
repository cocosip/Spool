using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spool
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddSpool(
            this IServiceCollection services,
            Action<SpoolOptions> configure = null)
        {
            configure ??= new Action<SpoolOptions>(options => { });

            services.Configure(configure);

            services
                .AddSingleton<IFilePoolFactory, DefaultFilePoolFactory>()
                .AddTransient<IFilePoolConfigurationSelector, DefaultFilePoolConfigurationSelector>()
                ;

            return services;
        }
    }
}
