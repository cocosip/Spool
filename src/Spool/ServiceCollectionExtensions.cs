﻿using Microsoft.Extensions.DependencyInjection;
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
                .AddSingleton<ISpoolPool, SpoolPool>()
                .AddSingleton<ISpoolHost, SpoolHost>()
                .AddSingleton<IdGenerator>()
                .AddSingleton<IFilePoolFactory, FilePoolFactory>()
                .AddScoped<IFilePool, FilePool>()
                .AddScoped<FilePoolOption>()
                .AddTrains()
                .AddFileWriters()
                ;
            return services;
        }

        /// <summary>添加文件写入器
        /// </summary>
        internal static IServiceCollection AddFileWriters(this IServiceCollection services)
        {
            services
                .AddSingleton<IFileWriterBuilder, FileWriterBuilder>()
                .AddScoped<IFileWriterPool, FileWriterPool>()
                .AddScoped<IFileWriter, FileWriter>()
                .AddScoped<FileWriterOption>();
            return services;
        }

        /// <summary>添加序列
        /// </summary>
        internal static IServiceCollection AddTrains(this IServiceCollection services)
        {
            services
                .AddSingleton<ITrainBuilder, TrainBuilder>()
                .AddScoped<ITrainManager, TrainManager>()
                .AddScoped<ITrain, Train>()
                .AddScoped<TrainOption>()
                ;
            return services;
        }



    }
}
