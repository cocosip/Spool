using System.Collections.Generic;

namespace Spool
{
    /// <summary>配置信息
    /// </summary>
    public class SpoolOption
    {
        /// <summary>存储根目录,默认数据等存储路径
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>默认分组
        /// </summary>
        public string DefaultGroup { get; set; }

        /// <summary>最大序列的数量(默认999999)
        /// </summary>
        public int MaxTrainCount { get; set; } = 999999;

        /// <summary>每个序列下最大的文件数量(默认65534)
        /// </summary>
        public int MaxTrainFileCount { get; set; } = 65534;

        /// <summary>每个组文件池下最大文件写入的线程数
        /// </summary>
        public int GroupMaxFileWriterCount { get; set; }

        /// <summary>文件较大时,文件写入的每个分片的大小(默认5Mb)
        /// </summary>
        public int FileWriteSliceSize { get; set; } = 1024 * 1024 * 5;
        
        /// <summary>每个GroupPool下在在到内存队列中的最大的数量
        /// </summary>
        public int MaxSpoolFileQueueCount { get; set; } = 100000;

        /// <summary>分组
        /// </summary>
        public List<SpoolGroupDescriptor> Groups { get; set; }

        /// <summary>初始化基本配置
        /// </summary>
        public SpoolOption()
        {
            Groups = new List<SpoolGroupDescriptor>();
        }

    }
}
