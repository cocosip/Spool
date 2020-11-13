using Microsoft.Extensions.Options;

namespace Spool
{
    /// <summary>
    /// File pool configuration selector from configuration
    /// </summary>
    public class DefaultFilePoolConfigurationSelector : IFilePoolConfigurationSelector
    {
        private readonly SpoolOptions _options;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options"></param>
        public DefaultFilePoolConfigurationSelector(IOptions<SpoolOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Get file pool configuration by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FilePoolConfiguration Get(string name)
        {
            return _options.FilePools.GetConfiguration(name);
        }

    }
}
