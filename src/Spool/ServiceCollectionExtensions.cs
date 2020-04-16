using Microsoft.Extensions.DependencyInjection;
using Spool.Scheduling;
using Spool.Trains;
using Spool.Utility;
using Spool.Writers;
using System;

namespace Spool
{
    /// <summary>依赖注入扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>添加Spool
        /// </summary>
        public static IServiceCollection AddSpool(this IServiceCollection services, SpoolOption option)
        {

            void configure(SpoolOption o)
            {
                o.DefaultPool = option.DefaultPool;
                o.FilePools = option.FilePools;
            }

            return services.AddSpool(configure);
        }


        /// <summary>添加Spool
        /// </summary>
        public static IServiceCollection AddSpool(this IServiceCollection services, Action<SpoolOption> configure = null)
        {
            if (configure == null)
            {
                configure = o =>
                {

                };
            }
            services
                .AddSingleton<IScheduleService, ScheduleService>()
                .Configure<SpoolOption>(configure)
                .AddSingleton<SpoolPool>()
                .AddSingleton<ISpoolHost, SpoolHost>()
                .AddSingleton<IdGenerator>()
                .AddSingleton<IFilePoolFactory, FilePoolFactory>()
                .AddScoped<FilePool>()
                .AddScoped<FilePoolOption>()
                .AddScoped<ITrainManager, TrainManager>()
                .AddScoped<IFileWriterManager, FileWriterManager>()
                .AddScoped<FileWriter>()
                .AddScoped<Train>()
                .AddScoped<TrainOption>();
            return services;
        }
    }
}
