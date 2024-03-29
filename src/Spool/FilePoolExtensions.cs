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
        public static async ValueTask<SpoolFile> WriteFileAsync(this IFilePool filePool, string fileName)
        {
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
            var ext = FilePathUtil.GetPathExtension(fileName);
            var spoolFile = await filePool.WriteFileAsync(fs, ext);
            return spoolFile;
        }
    }
}
