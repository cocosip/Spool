namespace Spool.Trains
{
    /// <summary>
    /// Train factory
    /// </summary>
    public interface ITrainFactory
    {
        /// <summary>
        /// Create a new train
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        ITrain Create(FilePoolConfiguration configuration, int index);

    }
}
