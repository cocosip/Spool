namespace Spool.Trains
{
    /// <summary>
    /// Train type
    /// </summary>
    public enum TrainType
    {
        /// <summary>
        /// Default(unused)
        /// </summary>
        Default = 1,

        /// <summary>
        /// Read only
        /// </summary>
        Read = 2,

        /// <summary>
        /// Read and write
        /// </summary>
        ReadWrite = 4,

        /// <summary>
        /// Write
        /// </summary>
        Write = 8
    }
}
