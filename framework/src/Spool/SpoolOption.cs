using System.Collections.Generic;

namespace Spool
{
    /// <summary>配置信息
    /// </summary>
    public class SpoolOption
    {
        /// <summary>存储根目录,默认文件都会存储到该目录下,当目录存储数量达到一定的阈值的时候,会存储到扩展目录下
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>默认分组
        /// </summary>
        public string DefaultGroup { get; set; }

        /// <summary>每个组文件池下最大文件写入的线程数
        /// </summary>
        public int GroupMaxFileWriters { get; set; }

        /// <summary>监控目录
        /// </summary>
        public string WatcherPath { get; set; }

        /// <summary>扩展目录
        /// </summary>
        public List<string> ExtPaths { get; set; }

        /// <summary>初始化基本配置
        /// </summary>
        public SpoolOption()
        {
            RootPath = @"D:\Spool";
            DefaultGroup = "Default";
            WatcherPath = @"D\Spool\Watcher";
            ExtPaths = new List<string>();
        }

    }
}
