using System;
using System.IO;
using System.Threading.Tasks;

namespace Spool.Trains
{
    /// <summary>序列
    /// </summary>
    public interface ITrain
    {
        /// <summary>序列删除事件
        /// </summary>
        event EventHandler<TrainDeleteEventArgs> OnDelete;

        /// <summary>序列类型转换事件
        /// </summary>
        event EventHandler<TrainTypeChangeEventArgs> OnTypeChange;

        /// <summary>序列写满
        /// </summary>
        event EventHandler<TrainWriteOverEventArgs> OnWriteOver;

        /// <summary>序列的索引
        /// </summary>
        int Index { get; }

        /// <summary>序列名称
        /// </summary>
        string Name { get; }

        /// <summary>序列的路径
        /// </summary>
        string Path { get; }

        /// <summary>序列类型
        /// </summary>
        TrainType TrainType { get; }

        /// <summary>当前序列下待处理的数量
        /// </summary>
        int PendingCount { get; }

        /// <summary>当前序列下被取走的数量
        /// </summary>
        int ProgressingCount { get; }

        /// <summary>初始化
        /// </summary>
        void Initialize();

        /// <summary>序列信息
        /// </summary>
        string Info();

        /// <summary>能否释放
        /// </summary>
        bool IsEmpty();

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        Task<SpoolFile> WriteFileAsync(Stream stream, string fileExt);

        /// <summary>写文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <returns></returns>
        SpoolFile WriteFile(Stream stream, string fileExt);

        /// <summary>获取指定数量的文件
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        SpoolFile[] GetFiles(int count = 1);

        /// <summary>归还数据
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        void ReturnFiles(params SpoolFile[] spoolFiles);

        /// <summary>释放文件
        /// </summary>
        /// <param name="spoolFiles">文件列表</param>
        void ReleaseFiles(params SpoolFile[] spoolFiles);

        /// <summary>序列类型转换
        /// </summary>
        void ChangeType(TrainType type);
    }
}
