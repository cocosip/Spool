namespace Spool
{
    /// <summary>
    /// File pool configuration info
    /// </summary>
    public class FilePoolConfiguration
    {
        /// <summary>
        /// File pool name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Store path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Write file buffer size
        /// </summary>
        public int WriteBufferSize { get; set; } = 1024 * 1024 * 5;

        /// <summary>
        /// Max file count in one train , if exceed this number, it will create a new train
        /// </summary>
        public int TrainMaxFileCount { get; set; } = 65535;

        /// <summary>
        /// Enable file watcher
        /// </summary>
        public bool EnableFileWatcher { get; set; }

        /// <summary>
        /// File watcher path
        /// </summary>
        public string FileWatcherPath { get; set; }

        /// <summary>
        /// Copy watcher folder file to file pool thread
        /// </summary>
        public int FileWatcherCopyThread { get; set; }

        /// <summary>
        /// Scan file watecher path interval(ms)
        /// </summary>
        public int ScanFileWatcherMillSeconds { get; set; } = 5000;

        /// <summary>
        /// Enable automatic return file to file pool
        /// </summary>
        public bool EnableAutoReturn { get; set; }

        /// <summary>
        /// Scan wait return file interval(ms)
        /// </summary>
        public int ScanReturnFileMillSeconds { get; set; }

        /// <summary>
        /// Reutrn file expired time(s), beyond this time will automatic return file
        /// </summary>
        public int AutoReturnSeconds { get; set; }
    }
}
