using System;

namespace Spool
{
    /// <summary>归还文件事件
    /// </summary>
    public class ReturnFileEventArgs : EventArgs
    {
        /// <summary>文件池名称
        /// </summary>
        public string FilePoolName { get; set; }

        /// <summary>被删除的文件
        /// </summary>
        public SpoolFile[] SpoolFiles { get; set; }
    }
}
