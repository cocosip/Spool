using Spool.Trains;
using System;

namespace Spool.Events
{
    /// <summary>
    /// Train write over event args
    /// </summary>
    public class TrainWriteOverEventArgs : EventArgs
    {
        /// <summary>
        /// Train
        /// </summary>
        public TrainInfo Train { get; set; }
    }
}
