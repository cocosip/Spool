using Microsoft.Extensions.Options;

namespace Spool
{
    /// <summary>
    /// 文件池配置筛选器
    /// </summary>
    public class DefaultFilePoolConfigurationSelector : IFilePoolConfigurationSelector
    {
        protected SpoolOptions Options { get; }
        public DefaultFilePoolConfigurationSelector(IOptions<SpoolOptions> options)
        {
            Options = options.Value;
        }

        /// <summary>
        /// 根据名称获取文件池配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual FilePoolConfiguration Get(string name)
        {
            return Options.FilePools.GetConfiguration(name);
        }

    }
}
