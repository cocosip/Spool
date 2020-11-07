using System;
using System.Collections.Generic;
using System.Text;

namespace Spool
{
    /// <summary>
    /// SpoolFile future
    /// </summary>
    public class SpoolFileFuture
    {
        /// <summary>
        /// SpoolFile
        /// </summary>
        public SpoolFile File { get; set; }

        /// <summary>
        /// Begin time
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// Timeout seconds
        /// </summary>
        public long TimeoutSeconds { get; private set; }

        /// <summary>Ctor
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
