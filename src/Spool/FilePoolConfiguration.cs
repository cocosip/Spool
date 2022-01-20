using System;

namespace Spool
{
    [Serializable]
    public class FilePoolConfiguration
    {
        /// <summary>
        /// 文件池的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件池的存储路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 每个Worker下的存储的最大的文件数量,当超过该值时,将不会再写入。默认:5000
        /// </summary>
        public int WorkerMaxFile { get; set; } = 5000;

        /// <summary>
        /// 用来进行读的Worker的数量,默认:3
        /// </summary>
        public int ReadWorkerCount { get; set; } = 3;

        /// <summary>
        /// 用来写入的Worker的数量,默认:3
        /// </summary>
        public int WriteWorkerCount { get; set; } = 3;

        /// <summary>
        /// 是否开启自动归还文件的操作,当开启之后,如果没有主动调用释放文件的方法,在超过指定的时间后<see cref="AutoReturnSeconds"/>,将会自动归还(重新放到队列中)。 默认:false
        /// </summary>
        public bool EnableAutoReturn { get; set; } = false;

        /// <summary>
        /// 自动归还的时间,以秒为单位。默认:300s
        /// </summary>
        public int AutoReturnSeconds { get; set; } = 300;

        /// <summary>
        /// 扫描过期的文件的时间,以毫秒为单位。默认:3000ms
        /// </summary>
        public int ReturnFileScanMillis { get; set; } = 3000;

        /// <summary>
        /// 是否开启文件监控。默认:false
        /// </summary>
        public bool EnableFileWatcher { get; set; } = false;

        /// <summary>
        /// 文件监控的目录
        /// </summary>
        public bool FileWatcherPath { get; set; }

        /// <summary>
        /// 扫描文件监控目录的时间间隔,以毫秒为单位。默认:5000ms
        /// </summary>
        public int FileWatcherScanIntervalMillis { get; set; } = 5000;

        /// <summary>
        /// 文件监控工作线程数,默认:3
        /// </summary>
        public int FileWatcherWorkThread { get; set; } = 3;

        /// <summary>
        /// 是否跳过拷贝文件大小为0KB的文件。默认:true
        /// </summary>
        public bool FileWatcherSkipZeroFile { get; set; } = true;

        /// <summary>
        /// 文件监控,读取时对数据的延迟,以秒为单位。默认:30s
        /// 只有在当前时间30秒之前的文件才会被拷贝
        /// </summary>
        public int FileWatcherReadDelaySeconds { get; set; } = 30;

    }
}