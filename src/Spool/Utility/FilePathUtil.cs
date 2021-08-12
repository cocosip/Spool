using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spool.Utility
{
    /// <summary>
    /// File path util
    /// </summary>
    public static class FilePathUtil
    {
        /// <summary>
        /// Get file path extension
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathExtension(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && path.IndexOf('.') >= 0)
            {
                return path[path.LastIndexOf('.')..];
            }
            return string.Empty;
        }

        /// <summary>
        /// Create directory if not exist
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>Create success or not</returns>
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
        /// <returns>Delete success or not</returns>
        public static bool DeleteDirIfExist(string directory, bool recursive = false)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete file if exist
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Delete success or not</returns>
        public static bool DeleteFileIfExists(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Recursive get files
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


        /// <summary>
        /// Is same root path
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static bool SameRootPath(string path1, string path2)
        {
            return Path.GetPathRoot(path1) == Path.GetPathRoot(path2);
        }

    }
}
