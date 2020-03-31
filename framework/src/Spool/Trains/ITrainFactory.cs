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

        /// <summary>获取序列索引最新的序列
        /// </summary>
        Train GetLatest(IEnumerable<Train> trains);

        /// <summary>根据序列信息,文件池配置信息获取序列基本信息
        /// </summary>
        TrainInfo BuildInfo(Train train, FilePoolOption option);
    }
}
