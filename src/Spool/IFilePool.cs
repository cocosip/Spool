using System;

namespace Spool
{
    public interface IFilePool<TFilePool> : IFilePool
        where TFilePool : class
    {


    }

    public interface IFilePool : IDisposable
    {
        /// <summary>
        /// 初始化设置
        /// </summary>
        void Setup();
    }
}
