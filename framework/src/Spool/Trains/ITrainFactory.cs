using System.Collections.Generic;

namespace Spool.Trains
{
    /// <summary>序列工厂
    /// </summary>
    public interface ITrainFactory
    {
        /// <summary>获取路径下的序列
        /// </summary>
        List<Train> GetTrainsFromPath(FilePoolOption option);

        /// <summary>创建序列
        /// </summary>
        Train CreateTrain(FilePoolOption option, int index);
    }
}
