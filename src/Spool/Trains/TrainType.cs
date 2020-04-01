namespace Spool.Trains
{
    /// <summary>序列类型
    /// </summary>
    public enum TrainType
    {
        /// <summary>默认(未分配)
        /// </summary>
        Default = 1,

        /// <summary>只读
        /// </summary>
        Read = 2,

        /// <summary>能读能写
        /// </summary>
        ReadWrite = 4,

        /// <summary>只写
        /// </summary>
        Write = 8
    }
}
