using System;

namespace Spool
{
    /// <summary>
    /// 文件池配置信息
    /// </summary>
    [Serializable]
    public class FilePoolConfiguration
    {
        /// <summary>
        /// 文件池名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 存储路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 写入的字节大小,默认:5MB
        /// </summary>
        public int WriteBufferSize { get; set; } = 1024 * 1024 * 5;

        /// <summary>
        /// 每个Worker最大的文件数量,默认:10000
        /// 超过该数量,会开始写下一个
        /// </summary>
        public int MaxFileCount { get; set; } = 10000;

        /// <summary>
        /// 是否开启文件监测,默认:false
        /// </summary>
        public bool EnableFileWatcher { get; set; } = false;

        /// <summary>
        /// File watcher path
        /// </summary>
        public string FileWatcherPath { get; set; }

        /// <summary>
        /// 拷贝监控的目录的线程数量,默认:1
        /// </summary>
        public int FileWatcherCopyThread { get; set; } = 1;

        /// <summary>
        /// 监控目录最后写入时间,默认:30s
        /// 文件写入超过30s之后,才会被拷贝,避免因为没有写入完成就被拷贝到文件池
        /// </summary>
        public int FileWatcherLastWrite { get; set; } = 30;

        /// <summary>
        /// 是否跳过文件大小为0KB的文件,默认:true
        /// 如果文件为0KB不会写入到文件池
        /// </summary>
        public bool FileWatcherSkipZeroFile { get; set; } = true;

        /// <summary>
        /// 扫描监控目录的时间间隔,默认:5s
        /// </summary>
        public int ScanFileWatcherMillSeconds { get; set; } = 5000;

        /// <summary>
        /// 是否开启自动归还文件的功能,默认:false
        /// 当开启后,在指定的时间内,没有归还文件,会自动重新放到待处理的队列中
        /// </summary>
        public bool EnableAutoReturn { get; set; } = false;

        /// <summary>
        /// 扫描待归还文件的时间间隔,默认:3s
        /// </summary>
        public int ScanReturnFileMillSeconds { get; set; } = 3000;

        /// <summary>
        /// 自动归还文件的超时时间,默认:300s
        /// 超过300s的文件没有处理(归还或者释放)就进行自动归还
        /// </summary>
        public int AutoReturnSeconds { get; set; } = 300;
    }
}
