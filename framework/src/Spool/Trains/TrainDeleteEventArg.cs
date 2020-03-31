using System;

namespace Spool.Trains
{
    /// <summary>序列删除事件
    /// </summary>
    public class TrainDeleteEventArg : EventArgs
    {
        /// <summary>组信息
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>组路径
        /// </summary>
        public string GroupPath { get; set; }

        /// <summary>序列的索引
        /// </summary>
        public int Index { get; set; }
    
    }
}
