using Microsoft.Extensions.DependencyInjection;
using Spool.Utility;
using Spool.Writer;
using System;
using System.Collections.Generic;
using System.Text;

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
                .AddSingleton<SpoolOption>(option)
                .AddSingleton<FilePool>();

            return services;
        }
    }
}
