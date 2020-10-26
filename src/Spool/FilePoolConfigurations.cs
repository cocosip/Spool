using System;
using System.Collections.Generic;
using System.Text;

namespace Spool
{
    /// <summary>
    /// File pool configuration aggregation
    /// </summary>
    public class FilePoolConfigurations
    {
        private FilePoolConfiguration Default => GetConfiguration<DefaultFilePool>();

        private readonly Dictionary<string, FilePoolConfiguration> _filePools;

        /// <summary>
        /// Ctor
        /// </summary>
        public FilePoolConfigurations()
        {
            _filePools = new Dictionary<string, FilePoolConfiguration>()
            {
                //Add default container
                [FilePoolNameAttribute.GetFilePoolName<DefaultFilePool>()] = new FilePoolConfiguration()
            };
        }

        /// <summary>
        /// Configure file pool 
        /// </summary>
        /// <typeparam name="TFilePool"></typeparam>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public FilePoolConfigurations Configure<TFilePool>(
           Action<FilePoolConfiguration> configureAction)
        {
            return Configure(
                FilePoolNameAttribute.GetFilePoolName<TFilePool>(),
                configureAction
            );
        }

        /// <summary>
        /// Configure file pool 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public FilePoolConfigurations Configure(
             string name,
             Action<FilePoolConfiguration> configureAction)
        {

            if (!_filePools.TryGetValue(name, out FilePoolConfiguration configuration))
            {
                configuration = new FilePoolConfiguration();
                _filePools.Add(name, configuration);
            }

            configureAction(configuration);

            return this;
        }

        /// <summary>
        /// Configure default file pool 
        /// </summary>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public FilePoolConfigurations ConfigureDefault(Action<FilePoolConfiguration> configureAction)
        {
            configureAction(Default);
            return this;
        }

        /// <summary>
        /// Configure all file pools 
        /// </summary>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public FilePoolConfigurations ConfigureAll(Action<string, FilePoolConfiguration> configureAction)
        {
            foreach (var filePool in _filePools)
            {
                configureAction(filePool.Key, filePool.Value);
            }

            return this;
        }

        /// <summary>
        /// Get file pool configuration
        /// </summary>
        /// <typeparam name="TFilePool"></typeparam>
        /// <returns></returns>
        public FilePoolConfiguration GetConfiguration<TFilePool>()
        {
            return GetConfiguration(FilePoolNameAttribute.GetFilePoolName<TFilePool>());
        }

        /// <summary>
        /// Get file pool configuration by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FilePoolConfiguration GetConfiguration(string name)
        {
            _filePools.TryGetValue(name, out FilePoolConfiguration configuration);
            return configuration ?? Default;
        }

    }
}
