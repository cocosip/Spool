namespace Spool.Trains
{
    /// <summary>序列信息
    /// </summary>
    public class TrainInfo
    {
        /// <summary>文件池名称
        /// </summary>
        public string FilePoolName { get; set; }

        /// <summary>文件池路径
        /// </summary>
        public string FilePoolPath { get; set; }

        /// <summary>序列索引号
        /// </summary>
        public int Index { get; set; }

        /// <summary>序列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>序列路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>序列类型
        /// </summary>
        public TrainType TrainType { get; set; }

    }
}
