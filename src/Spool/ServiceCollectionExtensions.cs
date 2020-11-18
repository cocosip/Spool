using Microsoft.Extensions.DependencyInjection;
using Spool.Trains;
using Spool.Utility;
using System;

namespace Spool
{
    /// <summary>
    /// Dependency injection extensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add file pool injection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddSpool(this IServiceCollection services, Action<SpoolOptions> configure = null)
        {
            configure ??= new Action<SpoolOptions>(c => { });

            services.Configure(configure);

            services
                .AddSingleton<IScheduleService, ScheduleService>()
                .AddSingleton<IFilePoolFactory, DefaultFilePoolFactory>()
                .AddTransient(typeof(IFilePool<>), typeof(FilePool<>))
                .AddTransient<ITrainFactory, DefaultTrainFactory>()
                .AddTransient<IFilePoolConfigurationSelector, DefaultFilePoolConfigurationSelector>();

            return services;
        }
    }
}
