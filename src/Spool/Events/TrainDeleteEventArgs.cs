using Spool.Trains;
using System;

namespace Spool.Events
{
    /// <summary>
    /// Train delete event args
    /// </summary>
    public class TrainDeleteEventArgs : EventArgs
    {
        /// <summary>
        /// Train
        /// </summary>
        public TrainInfo Train { get; set; }
    }
}
