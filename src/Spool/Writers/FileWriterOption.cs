namespace Spool.Writers
{
    /// <summary>文件写入器配置信息
    /// </summary>
    public class FileWriterOption
    {
        /// <summary>名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>存储路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>最大的写入数(当数据为0时不限制)
        /// </summary>
        public int MaxFileWriterCount { get; set; }

        /// <summary>文件写入器的并发线程数
        /// </summary>
        public int ConcurrentFileWriterCount { get; set; }

        /// <summary>写入文件缓存大小
        /// </summary>
        public int WriteBufferSize { get; set; }
    }
}
