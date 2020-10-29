using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Spool
{
    public class FilePoolFactory : IFilePoolFactory
    {
        private readonly ILogger _logger;
        private readonly IFilePoolConfigurationSelector _configurationSelector;

        private readonly object _sync = new object();
        private readonly ConcurrentDictionary<string, IFilePool> _filePools;
        public FilePoolFactory(ILogger<FilePoolFactory> logger, IFilePoolConfigurationSelector configurationSelector)
        {
            _logger = logger;
            _configurationSelector = configurationSelector;
            _filePools = new ConcurrentDictionary<string, IFilePool>();
        }


        public IFilePool GetOrCreate(string name)
        {
            if (!_filePools.TryGetValue(name, out IFilePool filePool))
            {
                lock (_sync)
                {
                    if (!_filePools.TryGetValue(name, out filePool))
                    {
                        //创建文件池
                        var configuration = _configurationSelector.Get(name);
                        filePool = new FilePool();


                    }
                }
            }

            return filePool;
        }

        protected virtual IFilePool BuildFilePool(string name)
        {
            var configuration = _configurationSelector.Get(name);
            IFilePool filePool = new FilePool();

            return filePool;
        }

    }
}
