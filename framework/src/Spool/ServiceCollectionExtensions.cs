using Microsoft.Extensions.DependencyInjection;
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

            services
                .AddSingleton<SpoolOption>(option)
                .AddSingleton<SpoolPool>()
                .AddSingleton<ISpoolHost, SpoolHost>()
                .AddSingleton<IdGenerator>()
                .AddSingleton<IFilePoolFactory, FilePoolFactory>()
                .AddSingleton<ITrainFactory, TrainFactory>()
                .AddScoped<FilePool>()
                .AddScoped<FilePoolOption>()
                .AddScoped<IFileWriterManager, FileWriterManager>()
                .AddScoped<FileWriter>()

                ;

            return services;
        }
    }
}
