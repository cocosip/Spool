using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>
    /// FilePool
    /// </summary>
    /// <typeparam name="TFilePool"></typeparam>
    public interface IFilePool<TFilePool> : IFilePool
    where TFilePool : class
    {

    }


    /// <summary>
    /// FilePool
    /// </summary>
    public interface IFilePool : IDisposable
    {
        /// <summary>
        /// Setup file pool
        /// </summary>
        void Setup();

        /// <summary>
        /// Gets the specified number of files
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        List<SpoolFile> GetFiles(int count = 1);

        /// <summary>
        /// Write file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        Task<SpoolFile> WriteFileAsync(Stream stream, string fileExt);

        /// <summary>
        /// Write file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        SpoolFile WriteFile(Stream stream, string fileExt);

        /// <summary>
        /// Return files to file pool
        /// </summary>
        /// <param name="files"></param>
        void ReturnFiles(params SpoolFile[] files);

        /// <summary>
        /// Release files
        /// </summary>
        /// <param name="files"></param>
        void ReleaseFiles(params SpoolFile[] files);

        /// <summary>
        /// Get pending files count
        /// </summary>
        /// <returns></returns>
        int GetPendingCount();

        /// <summary>
        /// Get processing files count
        /// </summary>
        int GetProcessingCount();
    }
}
