using System;
using System.Collections.Generic;
using System.Text;

namespace Spool.Writers
{
    /// <summary>文件写入管理器
    /// </summary>
    public interface IFileWriterManager
    {

        /// <summary>Get a file writer
        /// </summary>
        FileWriter Get();

        /// <summary>Return a file writer
        /// </summary>
        public void Return(FileWriter fileWriter);
    }
}
