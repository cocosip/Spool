using System;

namespace Spool.Trains
{
    /// <summary>在序列删除之后,还有文件归还的操作
    /// </summary>
    public class TrainDeleteReturnFilesEventArgs : EventArgs
    {
        /// <summary>序列基本信息
        /// </summary>
        public TrainInfo Info { get; set; }

        /// <summary>文件数量
        /// </summary>
        public int FileCount { get; set; }

    }
}
