using System;

namespace Spool.Trains
{
    /// <summary>序列类型转换时的数据
    /// </summary>
    public class TrainTypeChangeEventArgs : EventArgs
    {
        /// <summary>序列基本信息
        /// </summary>
        public TrainInfo Info { get; set; }

        /// <summary>转换前序列类型
        /// </summary>
        public TrainType SourceType { get; set; }
    }

   
}
