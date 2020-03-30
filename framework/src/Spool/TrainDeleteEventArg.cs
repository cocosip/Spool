using System;

namespace Spool
{
    /// <summary>序列删除事件
    /// </summary>
    public class TrainDeleteEventArg : EventArgs
    {
        /// <summary>组信息
        /// </summary>
        public GroupDescriptor Group { get; set; }

        /// <summary>序列的索引
        /// </summary>
        public int Index { get; set; }
    }
}
