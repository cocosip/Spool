namespace Spool.Writer
{
    /// <summary>File writer info
    /// </summary>
    public class FileWriterOption
    {
        /// <summary>Group name
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>The max file writer count
        /// </summary>
        public int MaxFileWriterCount { get; set; } = 10;

        /// <summary>The file write item slice size
        /// </summary>
        public int FileWriteSliceSize { get; set; } = 1024 * 1024 * 5;



        public override string ToString()
        {
            return $"FileWriterOption-[GroupName:{GroupName},MaxFileWriterCount:{MaxFileWriterCount}]";
        }
    }
}
