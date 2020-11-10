using Spool.Trains;
using System;

namespace Spool.Events
{
    /// <summary>
    /// Train type change event args
    /// </summary>
    public class TrainTypeChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Train info
        /// </summary>
        public TrainInfo Train { get; set; }

        /// <summary>
        /// Source type
        /// </summary>
        public TrainType SourceType { get; set; }

        /// <summary>
        /// Destination type
        /// </summary>
        public TrainType DestinationType { get; set; }
    }
}
