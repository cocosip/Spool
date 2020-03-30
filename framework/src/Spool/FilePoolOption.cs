using System;
using System.Collections.Generic;
using System.Text;

namespace Spool
{
    /// <summary>文件池配置
    /// </summary>
    public class FilePoolOption
    {
        /// <summary>文件池名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>文件池存储文件的根路径
        /// </summary>
        public string RootPath { get; set; }


    }
}
