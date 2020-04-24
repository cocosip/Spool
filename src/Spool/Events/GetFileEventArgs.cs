using System;

namespace Spool
{
    /// <summary>文件被取走事件
    /// </summary>
    public class GetFileEventArgs : EventArgs
    {
        /// <summary>文件池名称
        /// </summary>
        public string FilePoolName { get; set; }

        /// <summary>被取走的文件
        /// </summary>
        public SpoolFile[] SpoolFiles { get; set; }

        /// <summary>获取文件数量
        /// </summary>
        public int GetFileCount { get; set; }

    }
}
