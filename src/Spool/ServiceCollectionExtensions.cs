using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Spool.Workers;

namespace Spool
{
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Add spool
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddSpool(this IServiceCollection services, Action<SpoolOptions> configure = null)
        {

            services
                .AddSingleton<IFilePoolFactory, FilePoolFactory>()
                .AddTransient<IWorkerFactory, WorkerFactory>();

            var c = configure ?? new Action<SpoolOptions>(o => { });

            services.Configure<SpoolOptions>(c);

            return services;
        }

    }
}