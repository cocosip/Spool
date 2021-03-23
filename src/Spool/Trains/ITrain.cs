using Spool.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Spool.Trains
{
    /// <summary>
    /// Train
    /// </summary>
    public interface ITrain
    {

        /// <summary>
        /// Delete train event
        /// </summary>
        event EventHandler<TrainDeleteEventArgs> OnDelete;

        /// <summary>
        /// Train type change event
        /// </summary>
        event EventHandler<TrainTypeChangeEventArgs> OnTypeChange;

        /// <summary>
        /// Train write over event
        /// </summary>
        event EventHandler<TrainWriteOverEventArgs> OnWriteOver;

        /// <summary>
        /// File pool name
        /// </summary>
        string FilePool { get; }

        /// <summary>
        /// Train name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Train path
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Train index
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Train type
        /// </summary>
        TrainType TrainType { get; }

        /// <summary>
        /// Pending handle files
        /// </summary>
        int PendingCount { get; }

        /// <summary>
        /// Take away to handle files
        /// </summary>
        int ProgressingCount { get; }

        /// <summary>
        /// Initialize
        /// </summary>
        void Initialize();

        /// <summary>
        /// Train info
        /// </summary>
        string Info();

        /// <summary>
        /// Whether pending queue is empty
        /// </summary>
        /// <returns></returns>
        bool IsPendingEmpty();

        /// <summary>
        /// Write file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        Task<SpoolFile> WriteFileAsync(Stream stream, string fileExt);

        /// <summary>
        /// Gets the specified number of files
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        List<SpoolFile> GetFiles(int count = 1);

        /// <summary>
        /// Return files
        /// </summary>
        /// <param name="spoolFiles"></param>
        void ReturnFiles(params SpoolFile[] spoolFiles);

        /// <summary>
        /// Release files
        /// </summary>
        /// <param name="spoolFiles"></param>
        void ReleaseFiles(params SpoolFile[] spoolFiles);

        /// <summary>
        /// Change the train type
        /// </summary>
        /// <param name="type"></param>
        void ChangeType(TrainType type);
    }
}
