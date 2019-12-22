using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spool.Dependency
{
    public static class ServiceProviderExtensions
    {

        /// <summary>Create object
        /// </summary>
        public static object CreateInstance(this IServiceProvider provider, Type type, params object[] args)
        {
            return ActivatorUtilities.CreateInstance(provider, type, args);
        }

        /// <summary>Create object
        /// </summary>
        public static T CreateInstance<T>(this IServiceProvider provider, params object[] args)
        {
            return (T)ActivatorUtilities.CreateInstance(provider, typeof(T), args);
        }

    }
}
