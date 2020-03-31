﻿namespace Spool
{
    /// <summary>文件池配置
    /// </summary>
    public class FilePoolOption
    {
        /// <summary>文件池名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>文件池存储文件的根路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>最大的写入数
        /// </summary>
        public int MaxFileWriterCount { get; set; } = 10;

        /// <summary>写入文件缓存大小
        /// </summary>
        public int WriteBufferSize { get; set; } = 1024 * 1024 * 5;

        /// <summary>是否启动目录监控
        /// </summary>
        public bool EnableFileWatcher { get; set; } = false;

        /// <summary>监控目录
        /// </summary>
        public string FileWatcherPath { get; set; }

        /// <summary>是否启用自动归还功能(对长时间未归还也未删除的文件进行自动归还)
        /// </summary>
        public bool EnableAutoReturn { get; set; } = false;

        /// <summary>扫描待归还文件的间隔(毫秒)
        /// </summary>
        public int ScanReturnFileMillSeconds { get; set; } = 2000;

        /// <summary>自动归还时间秒(秒)
        /// </summary>
        public int AutoReturnSeconds { get; set; } = 600;

    }
}
