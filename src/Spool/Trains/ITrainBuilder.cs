using System.Collections.Generic;

namespace Spool.Trains
{
    /// <summary>序列创建器
    /// </summary>
    public interface ITrainBuilder
    {
        /// <summary>创建序列
        /// </summary>
        /// <param name="index">序列号</param>
        /// <param name="filePoolOption">文件池配置信息</param>
        /// <returns></returns>
        ITrain BuildTrain(int index, FilePoolOption filePoolOption);

        /// <summary>创建序列
        /// </summary>
        /// <param name="option">序列配置信息</param>
        /// <param name="filePoolOption">文件池配置信息</param>
        /// <returns></returns>
        ITrain BuildTrain(TrainOption option, FilePoolOption filePoolOption);

        /// <summary>创建已经存在的文件池下的序列
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        List<ITrain> BuildPoolTrains(FilePoolOption option);
    }
}
