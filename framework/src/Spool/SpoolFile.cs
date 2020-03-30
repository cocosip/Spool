﻿namespace Spool
{
    /// <summary>文件信息
    /// </summary>
    public class SpoolFile
    {
        /// <summary>组名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>序列的索引
        /// </summary>
        public int TrainIndex { get; set; }

        /// <summary>文件路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>文件信息
        /// </summary>
        public override string ToString()
        {
            return $"[Group:{GroupName},Index:{TrainIndex},Path:{Path}]";
        }
    }
}
