using System;
using System.Collections.Generic;

namespace Spool
{
    public class FilePoolConfigurations
    {
        private FilePoolConfiguration Default => GetConfiguration<DefaultFilePool>();

        private readonly Dictionary<string, FilePoolConfiguration> _filePools;

        public FilePoolConfigurations()
        {
            _filePools = new Dictionary<string, FilePoolConfiguration>()
            {
                //添加默认的文件池
                [FilePoolNameAttribute.GetFilePoolName<DefaultFilePool>()] = new FilePoolConfiguration()
            };
        }

        /// <summary>
        /// 根据泛型类型配置文件池
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
        /// 根据名称配置文件池
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
        /// 配置默认的文件池
        /// </summary>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public FilePoolConfigurations ConfigureDefault(Action<FilePoolConfiguration> configureAction)
        {
            configureAction(Default);
            return this;
        }

        /// <summary>
        /// 配置全部的文件池
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
        /// 根据泛型类型获取文件池配置
        /// </summary>
        /// <typeparam name="TFilePool"></typeparam>
        /// <returns></returns>
        public FilePoolConfiguration GetConfiguration<TFilePool>()
        {
            return GetConfiguration(FilePoolNameAttribute.GetFilePoolName<TFilePool>());
        }

        /// <summary>
        /// 根据名称获取文件池配置
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
