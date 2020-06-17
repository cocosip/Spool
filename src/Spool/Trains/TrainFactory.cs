using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Spool.Trains
{
    /// <summary>序列工厂
    /// </summary>
    public class TrainFactory : ITrainFactory
    {
        private readonly ManualResetEventSlim _manualResetEventSlim;

        /// <summary>Ctor
        /// </summary>
        public TrainFactory()
        {
            _manualResetEventSlim = new ManualResetEventSlim(true);
        }




    }
}
