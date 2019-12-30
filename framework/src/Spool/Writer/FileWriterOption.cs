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
        public int MaxFileWriterCount { get; set; }

        public override string ToString()
        {
            return $"FileWriterOption-[GroupName:{GroupName},MaxFileWriterCount:{MaxFileWriterCount}]";
        }
    }
}
