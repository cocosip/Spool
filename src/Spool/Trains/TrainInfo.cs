namespace Spool.Trains
{
    /// <summary>
    /// Train info
    /// </summary>
    public class TrainInfo
    {
        /// <summary>
        /// File pool name
        /// </summary>
        public string FilePool { get; set; }

        /// <summary>
        /// File pool path
        /// </summary>
        public string FilePoolPath { get; set; }

        /// <summary>
        /// Train index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Train name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Train path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Train type
        /// </summary>
        public TrainType TrainType { get; set; }
    }
}
