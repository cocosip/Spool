using Microsoft.Extensions.Options;

namespace Spool
{
    public class DefaultFilePoolConfigurationSelector : IFilePoolConfigurationSelector
    {
        private readonly SpoolOptions _options;

        public DefaultFilePoolConfigurationSelector(IOptions<SpoolOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// 根据名称获取文件池配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FilePoolConfiguration Get(string name)
        {
            return _options.FilePools.GetConfiguration(name);
        }

    }
}
