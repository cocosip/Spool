using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spool.Utility
{
    /// <summary>
    /// DirectoryHelper
    /// </summary>
    public static class DirectoryHelper
    {
        /// <summary>
        /// Create file directory if not exist
        /// </summary>
        /// <param name="directory"></param>
        public static bool CreateIfNotExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Delete directory if exist
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static bool DeleteIfExist(string directory, bool recursive = false)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get FileInfos from path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<FileInfo> RecursiveGetFileInfos(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var files = directoryInfo.GetFiles().ToList();
            var subDirs = directoryInfo.GetDirectories();
            foreach (var subDir in subDirs)
            {
                var subFiles = RecursiveGetFileInfos(subDir.FullName);
                files.AddRange(subFiles);
            }
            return files;
        }
    }
}
