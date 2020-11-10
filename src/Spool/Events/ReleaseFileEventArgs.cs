using System;
using System.Collections.Generic;

namespace Spool.Events
{
    /// <summary>
    /// Release file event args
    /// </summary>
    public class ReleaseFileEventArgs : EventArgs
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
        public ReleaseFileEventArgs()
        {
            Files = new List<SpoolFile>();
        }
    }
}
