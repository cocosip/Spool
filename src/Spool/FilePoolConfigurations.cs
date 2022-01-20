using System;
using System.Collections.Generic;
using Spool.Extensions;

namespace Spool
{
    public class FilePoolConfigurations
    {
        private FilePoolConfiguration Default => GetConfiguration<DefaultFilePool>();
        private readonly Dictionary<string, FilePoolConfiguration> _filePools;

        public FilePoolConfigurations()
        {
            _filePools = new Dictionary<string, FilePoolConfiguration>
            {
                [FilePoolNameAttribute.GetFilePoolName<DefaultFilePool>()] = new FilePoolConfiguration()
            };
        }

        public FilePoolConfigurations Configure<TFilePool>(
            Action<FilePoolConfiguration> configureAction)
        {
            return Configure(
                FilePoolNameAttribute.GetFilePoolName<TFilePool>(),
                configureAction
            );
        }

        public FilePoolConfigurations Configure(
            string name,
            Action<FilePoolConfiguration> configureAction)
        {
            configureAction(
                _filePools.GetOrAdd(
                    name,
                    () => new FilePoolConfiguration()
                )
            );

            return this;
        }

        public FilePoolConfigurations ConfigureDefault(Action<FilePoolConfiguration> configureAction)
        {
            configureAction(Default);
            return this;
        }

        public FilePoolConfigurations ConfigureAll(Action<string, FilePoolConfiguration> configureAction)
        {
            foreach (var filePool in _filePools)
            {
                configureAction(filePool.Key, filePool.Value);
            }

            return this;
        }

        public FilePoolConfiguration GetConfiguration<TFilePool>()
        {
            return GetConfiguration(FilePoolNameAttribute.GetFilePoolName<TFilePool>());
        }

        public FilePoolConfiguration GetConfiguration(string name)
        {
            return _filePools.GetOrDefault(name) ??
                   Default;
        }

    }
}