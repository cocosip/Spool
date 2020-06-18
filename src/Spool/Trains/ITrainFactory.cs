using System;
using System.Collections.Generic;

namespace Spool.Trains
{
    /// <summary>序列管理器
    /// </summary>
    public interface ITrainFactory
    {
        /// <summary>初始化
        /// </summary>
        void Initialize();

        /// <summary>根据序列信息,文件池配置信息获取序列基本信息
        /// </summary>
        TrainInfo BuildInfo(ITrain train);

        /// <summary>获取可以写的序列
        /// </summary>
        ITrain GetWriteTrain();

        /// <summary>获取可读的序列
        /// </summary>
        ITrain GetReadTrain();

        /// <summary>根据索引号获取序列
        /// </summary>
        ITrain GetTrainByIndex(int index);

        /// <summary>获取全部的序列
        /// </summary>
        List<ITrain> GetTrains(Func<ITrain, bool> predicate);
    }
}
