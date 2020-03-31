using System;

namespace Spool.Trains
{
    /// <summary>序列删除事件
    /// </summary>
    public class TrainDeleteEventArgs : EventArgs
    {
        /// <summary>序列基本信息
        /// </summary>
        public TrainInfo Info { get; set; }
    }
}
