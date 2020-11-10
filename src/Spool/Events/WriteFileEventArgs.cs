using System;

namespace Spool.Events
{
    /// <summary>
    /// Write file event args
    /// </summary>
    public class WriteFileEventArgs : EventArgs
    {
        /// <summary>
        /// File pool name
        /// </summary>
        public string FilePool { get; set; }

        /// <summary>
        /// SpoolFile
        /// </summary>
        public SpoolFile File { get; set; }

    }
}
