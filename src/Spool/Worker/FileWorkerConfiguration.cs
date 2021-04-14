using Spool.Utility;

namespace Spool.Worker
{
    /// <summary>
    /// FileWorker 配置信息
    /// </summary>
    public class FileWorkerConfiguration
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
        /// 写入文件的Buffer大小
        /// </summary>
        public int WriteBufferSize { get; set; }

        /// <summary>
        /// 最大的文件大小,当写入超过这个数值时,这个FileWorker将会变更成只读
        /// </summary>
        public int MaxFileCount { get; set; }

        /// <summary>
        /// 当前FileWorker序号
        /// </summary>
        public int Index { get; set; }

    }
}
