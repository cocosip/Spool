﻿namespace Spool
{
    /// <summary>文件池描述
    /// </summary>
    public class FilePoolDescriptor
    {
        /// <summary>名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>存储路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>最大的写入数
        /// </summary>
        public int MaxFileWriterCount { get; set; } = 10;

        /// <summary>写入文件缓存大小
        /// </summary>
        public int WriteBufferSize { get; set; } = 1024 * 1024 * 5;

        /// <summary>每个序列下最大的文件数,序列下的文件超过该数量时,就新增一个新的序列写入
        /// </summary>
        public int TrainMaxFileCount { get; set; } = 65535;

        /// <summary>是否启动目录监控
        /// </summary>
        public bool EnableFileWatcher { get; set; }

        /// <summary>监控目录
        /// </summary>
        public string FileWatcherPath { get; set; }

        /// <summary>是否启用自动归还功能(对长时间未归还也未删除的文件进行自动归还)
        /// </summary>
        public bool EnableAutoReturn { get; set; }

        /// <summary>扫描待归还文件的间隔(毫秒)
        /// </summary>
        public int ScanReturnFileMillSeconds { get; set; }

        /// <summary>自动归还时间秒(秒)
        /// </summary>
        public int AutoReturnSeconds { get; set; }
    }
}