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
        /// 文件池存储路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 文件池下每个FileWorker的文件数量,默认:5000个
        /// 当已经写入的文件数量超过该值时,将会使用新的FileWorker写入
        /// </summary>
        public int FileWorkerMaxFileCount { get; set; } = 5000;

        /// <summary>
        /// 是否开启文件的自动归还,默认:false
        /// 当开启文件自动归还的时候,文件没有手动归还或者释放掉,系统会自动进行归还,重新放回到待处理的队列中去,需要与ScanReturnFileMilliSeconds配合使用
        /// </summary>
        public bool EnableAutoReturn { get; set; } = false;

        /// <summary>
        /// 扫描待自动会还的文件的时间间隔,默认:3000ms
        /// </summary>
        public int ScanReturnFileMilliSeconds { get; set; } = 3000;

        /// <summary>
        /// 自动归还文件超时时间,默认:300s
        /// 只有文件被取走的时间,超过该时间间隔后,才会被自动归还
        /// </summary>
        public int AutoReturnTimeoutSeconds { get; set; } = 300;


        /// <summary>
        /// 是否启用文件监控,默认:true
        /// 当为ture的时候,将会监控指定的目录,一旦有文件写入都会被拷贝到文件池工作目录下,需要与FileWatcherPath配合使用
        /// </summary>
        public bool EnableFileWatcher { get; set; } = false;

        /// <summary>
        /// 文件监控的目录
        /// </summary>
        public string FileWatcherPath { get; set; }

        /// <summary>
        /// 文件监控在拷贝文件时,使用的线程数量,默认:1
        /// 适当调整该数值,能提升拷贝到FilePool工作目录的速度
        /// </summary>
        public int FileWatcherCopyThread { get; set; } = 1;

        /// <summary>
        /// 文件监控拷贝文件时,拷贝文件的时候,需要文件的最后写入时间之后进行拷贝,默认:30s
        /// 根据业务适当做调整,不适合太小。当文件的写入时间间隔比较大的时候(如:网络流下载速度比较慢的时候),容易造成文件读取不完整
        /// </summary>
        public int FileWatcherCopySince { get; set; } = 30;

        /// <summary>
        /// 文件监控拷贝文件时,是否跳过大小为0KB的文件,默认:true
        /// </summary>
        public bool FileWatcherSkipZeroFile { get; set; } = true;

        /// <summary>
        /// 文件监控扫描的时间间隔,默认:3000ms
        /// </summary>
        public int ScanFileWatcherMilliSeconds { get; set; } = 3000;


    }
}
