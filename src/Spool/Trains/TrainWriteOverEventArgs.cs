using System;

namespace Spool.Trains
{
    /// <summary>文件写满了
    /// </summary>
    public class TrainWriteOverEventArgs : EventArgs
    {
        /// <summary>序列基本信息
        /// </summary>
        public TrainInfo Info { get; set; }
    }
}
