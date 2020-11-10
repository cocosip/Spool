using System;
using System.Collections.Generic;

namespace Spool.Events
{
    /// <summary>
    /// Get file event args
    /// </summary>
    public class GetFileEventArgs : EventArgs
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
        /// Try to get files count
        /// </summary>
        public int GetFileCount { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public GetFileEventArgs()
        {
            Files = new List<SpoolFile>();
        }
    }
}
