using System;
using System.Collections.Generic;

namespace Spool.Events
{
    /// <summary>
    /// Return file event args
    /// </summary>
    public class ReturnFileEventArgs : EventArgs
    {
        /// <summary>
        /// File pool name
        /// </summary>
        public string FilePool { get; set; }

        /// <summary>
        /// SpoolFiles
        /// </summary>
        public List<SpoolFile> Files { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public ReturnFileEventArgs()
        {
            Files = new List<SpoolFile>();
        }
    }
}
