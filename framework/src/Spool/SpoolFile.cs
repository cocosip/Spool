namespace Spool
{
    /// <summary>Spool write file
    /// </summary>
    public class SpoolFile
    {
        /// <summary>UniqueId
        /// </summary>
        public long Id { get; set; }

        /// <summary>FileName
        /// </summary>
        public string FileName { get; set; }

        /// <summary>File extension
        /// </summary>
        public string Ext { get; set; }

        /// <summary>File save full path
        /// </summary>
        public string Path { get; set; }
    }
}
