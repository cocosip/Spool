namespace Spool.Worker
{
    /// <summary>
    /// 文件读写工作者,一个工作者保持一个目录
    /// </summary>
    public class DefaultFileWorker : IFileWorker
    {
        private WorkerStyle _style = WorkerStyle.None;
        private WorkerState _state = WorkerState.Pending;

        /// <summary>
        /// 工作者类型
        /// </summary>
        public WorkerStyle Style => _style;

        /// <summary>
        /// 工作者状态
        /// </summary>
        public WorkerState State => _state;



    }
}
