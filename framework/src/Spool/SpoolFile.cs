namespace Spool
{
    /// <summary>Spool write file
    /// </summary>
    public class SpoolFile
    {
        /// <summary>所属分组
        /// </summary>
        public string GroupName { get; set; }

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
