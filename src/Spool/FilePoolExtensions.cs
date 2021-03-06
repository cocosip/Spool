﻿using Spool.Utility;
using System.IO;
using System.Threading.Tasks;

namespace Spool
{
    /// <summary>
    /// FilePool extensions
    /// </summary>
    public static class FilePoolExtensions
    {
        /// <summary>
        /// Write file async
        /// </summary>
        /// <param name="filePool"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ValueTask<SpoolFile> WriteFileAsync(this IFilePool filePool, string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
            var ext = FilePathUtil.GetPathExtension(fileName);
            return filePool.WriteFileAsync(fs, ext);
        }
    }
}
