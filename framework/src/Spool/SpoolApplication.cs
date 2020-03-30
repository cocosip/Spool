using System;
using System.Collections.Generic;
using System.Text;

namespace Spool
{

    /// <summary>Spool全局依赖注入信息
    /// </summary>
    public class SpoolApplication : ISpoolApplication
    {
        public IServiceProvider Provider => throw new NotImplementedException();
    }
}
