using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spool
{
    public interface IFilePool<TFilePool> : IFilePool where TFilePool : class
    {

    }

    public interface IFilePool
    {

    }
}