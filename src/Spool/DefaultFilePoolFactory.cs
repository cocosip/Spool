using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Spool
{
    public class DefaultFilePoolFactory : IFilePoolFactory
    {
        protected ILogger Logger { get; }
        protected IFilePoolConfigurationSelector ConfigurationSelector { get; }

        private readonly object _sync = new();
        private readonly ConcurrentDictionary<string, IFilePool> _filePoolDict;

        public DefaultFilePoolFactory(
            ILogger<DefaultFilePoolFactory> logger,
            IFilePoolConfigurationSelector configurationSelector)
        {
            Logger = logger;
            ConfigurationSelector = configurationSelector;

            _filePoolDict = new ConcurrentDictionary<string, IFilePool>();
        }

        /// <summary>
        /// 获取或者创建一个文件池
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual IFilePool GetOrCreate(string name)
        {
            if (!_filePoolDict.TryGetValue(name, out IFilePool filePool))
            {
                lock (_sync)
                {
                    if (!_filePoolDict.TryGetValue(name, out filePool))
                    {
                        //根据名称创建文件池
                        filePool = CreateFilePool(name);
                        //TODO Setup 初始化

                        if (!_filePoolDict.TryAdd(name, filePool))
                        {
                            Logger.LogWarning("Add file pool to dict failed.");
                        }
                    }
                }
            }

            return filePool;
        }


        protected virtual IFilePool CreateFilePool(string name)
        {
            var configuration = ConfigurationSelector.Get(name);
            if (configuration == null)
            {
                throw new ArgumentNullException($"Could not find configuration by name '{name}',check your configuration.");
            }
            return default;
        }

    }
}
