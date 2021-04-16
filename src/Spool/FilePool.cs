using System;
using System.Threading.Tasks;

namespace Spool
{
    public class FilePool<TFilePool> : IFilePool<TFilePool> where TFilePool : class
    {
        private readonly IFilePool _filePool;
        public FilePool(IFilePoolFactory filePoolFactory)
        {
            _filePool = filePoolFactory.GetOrCreate<TFilePool>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Setup()
        {
            throw new NotImplementedException();
        }
    }

    public class FilePool : IFilePool
    {
        private bool _isSetup = false;






        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Setup()
        {
            throw new NotImplementedException();
        }


       
    }

}
