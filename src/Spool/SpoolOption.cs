using System.Collections.Generic;

namespace Spool
{
    /// <summary>Spool配置
    /// </summary>
    public class SpoolOption
    {
        /// <summary>默认文件池名
        /// </summary>
        public string DefaultPool { get; set; } = "Group1";

        /// <summary>多个文件池配置信息
        /// </summary>
        public List<FilePoolDescriptor> FilePools { get; set; }

        public SpoolOption()
        {
            FilePools = new List<FilePoolDescriptor>();
        }
    }
}
