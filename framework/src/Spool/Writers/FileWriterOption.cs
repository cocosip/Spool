namespace Spool.Writers
{
    /// <summary>文件写入配置
    /// </summary>
    public class FileWriterOption
    {
        /// <summary>组信息
        /// </summary>
        public GroupDescriptor Group { get; set; }

        /// <summary>最大的写入数
        /// </summary>
        public int MaxFileWriterCount { get; set; } = 10;

        /// <summary>写入文件缓存大小
        /// </summary>
        public int WriteBufferSize { get; set; } = 1024 * 1024 * 5;



        public override string ToString()
        {
            return $"FileWriterOption-[GroupName:{Group.Name},MaxFileWriterCount:{MaxFileWriterCount}]";
        }
    }
}
