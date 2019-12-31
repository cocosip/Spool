namespace Spool.Group
{
    /// <summary>GroupPool descriptor
    /// </summary>
    public class GroupPoolDescriptor
    {
        /// <summary>GroupName
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>Group path
        /// </summary>
        public string GroupPath { get; set; }

        public override string ToString()
        {
            return $"[GroupName:{GroupName},GroupPath:{GroupPath}]";
        }

    }
}
