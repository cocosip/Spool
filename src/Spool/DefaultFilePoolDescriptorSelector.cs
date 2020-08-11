using Microsoft.Extensions.Options;
using System.Linq;

namespace Spool
{
    /// <summary>
    /// 文件池配置信息筛选器
    /// </summary>
    public class DefaultFilePoolDescriptorSelector : IFilePoolDescriptorSelector
    {
        private readonly SpoolOption _option;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options"></param>
        public DefaultFilePoolDescriptorSelector(IOptions<SpoolOption> options)
        {
            _option = options.Value;
        }

        /// <summary>
        /// Get FilePoolDescriptor by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FilePoolDescriptor GetDescriptor(string name)
        {
            var filePoolDescriptor = _option.FilePools.FirstOrDefault(x => x.Name == name);
            return filePoolDescriptor;
        }

    }
}
