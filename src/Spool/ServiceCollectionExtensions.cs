using Microsoft.Extensions.DependencyInjection;
using Spool.Scheduling;
using Spool.Trains;
using Spool.Utility;
using Spool.Writers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Spool
{
    /// <summary>依赖注入扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>添加Spool
        /// </summary>
        public static IServiceCollection AddSpool(this IServiceCollection services, Action<SpoolOption> configure = null)
        {
            var option = new SpoolOption();
            configure?.Invoke(option);
            return services.AddSpool(option);
        }

 
        /// <summary>添加Spool
        /// </summary>
        public static IServiceCollection AddSpool(this IServiceCollection services, SpoolOption option)
        {
            services
                .AddSingleton<IScheduleService, ScheduleService>()
                .AddSingleton<SpoolOption>(option)
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
