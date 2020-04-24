using System;

namespace Spool
{
    /// <summary>Spool文件时序
    /// </summary>
    public class SpoolFileFuture
    {
        /// <summary>文件
        /// </summary>
        public SpoolFile File { get; set; }

        /// <summary>开始时间(被拿走的时间)
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>过期需要的秒数
        /// </summary>
        public long TimeoutSeconds { get; private set; }

        /// <summary>ctor
        /// </summary>
        public SpoolFileFuture()
        {
            BeginTime = DateTime.Now;
        }

        /// <summary>Ctor
        /// </summary>
        public SpoolFileFuture(SpoolFile file, int timeoutSeconds)
        {
            BeginTime = DateTime.Now;
            File = file;
            TimeoutSeconds = timeoutSeconds;
        }



        /// <summary>是否过期
        /// </summary>
        public bool IsTimeout()
        {
            return (DateTime.Now - BeginTime).TotalSeconds > TimeoutSeconds;
        }




    }
}
