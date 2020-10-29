using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Spool
{
    public class FilePool<TFilePool> : IFilePool<TFilePool> where TFilePool : class
    {

    }

    public class FilePool : IFilePool
    {
        public async Task WriteFileAsync()
        {

        }

    }
}
