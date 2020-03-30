using System;
using System.Collections.Generic;
using System.Text;

namespace Spool
{
    /// <summary>Spool全局依赖注入信息
    /// </summary>
    public interface ISpoolApplication
    {
        /// <summary>全局Provider
        /// </summary>
        IServiceProvider Provider { get; }
    }
}
