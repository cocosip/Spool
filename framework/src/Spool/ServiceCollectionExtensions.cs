using Microsoft.Extensions.DependencyInjection;
using Spool.Group;
using Spool.Scheduling;
using Spool.Utility;
using Spool.Writer;
using System;

namespace Spool
{
    /// <summary>Depente
    /// </summary>
    public static class ServiceCollectionExtensions
    {

        /// <summary>Add filePool,singleton
        /// </summary>
        public static IServiceCollection AddSpool(this IServiceCollection services, Action<SpoolOption> configure = null)
        {
            var option = new SpoolOption();
            configure?.Invoke(option);


            services
                .AddSingleton<IdGenerator>()
                .AddSingleton<IScheduleService, ScheduleService>()
                .AddSingleton<SpoolOption>(option)
                .AddSingleton<FilePool>()
                .AddTransient<IGroupPoolManager, GroupPoolManager>()
                .AddScoped<SpoolGroupDescriptor>()
                .AddScoped<ITrainManager, TrainManager>()
                .AddScoped<FileWriterOption>()
                .AddScoped<IFileWriterManager, FileWriterManager>()
                ;

            return services;
        }
    }
}
