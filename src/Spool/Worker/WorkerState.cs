using System;
using System.Collections.Generic;
using System.Text;

namespace Spool.Worker
{
    /// <summary>
    /// 文件工作者状态
    /// </summary>
    public enum WorkerState
    {
        /// <summary>
        /// 待处理(还未初始化)
        /// </summary>
        Pending = 1,

        /// <summary>
        /// 进行中
        /// </summary>
        Processing = 2,



    }
}
