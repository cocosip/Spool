namespace Spool.Trains
{
    /// <summary>序列管理器
    /// </summary>
    public interface ITrainManager
    {
        /// <summary>初始化
        /// </summary>
        void Initialize();

        /// <summary>根据序列信息,文件池配置信息获取序列基本信息
        /// </summary>
        TrainInfo BuildInfo(Train train, FilePoolOption option);

        /// <summary>获取可以写的序列
        /// </summary>
        Train GetWriteTrain();

        /// <summary>获取可读的序列
        /// </summary>
        Train GetReadTrain();

        /// <summary>根据索引号获取序列
        /// </summary>
        Train GetTrainByIndex(int index);
    }
}
