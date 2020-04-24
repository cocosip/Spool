using System;

namespace Spool
{
    /// <summary>写文件事件
    /// </summary>
    public class WriteFileEventArgs : EventArgs
    {
        /// <summary>文件池名称
        /// </summary>
        public string FilePoolName { get; set; }

        /// <summary>写入的文件
        /// </summary>
        public SpoolFile SpoolFile { get; set; }

    }
}
