namespace Spool.Trains
{
    /// <summary>序列创建器
    /// </summary>
    public interface ITrainBuilder
    {
        /// <summary>创建序列
        /// </summary>
        /// <param name="option">序列配置信息</param>
        /// <param name="filePoolOption">文件池配置信息</param>
        /// <returns></returns>
        ITrain BuildTrain(TrainOption option, FilePoolOption filePoolOption);
    }
}
