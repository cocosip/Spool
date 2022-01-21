using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spool.Utils
{
    public class PathUtil
    {
        public static bool CreateIfNotExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                return true;
            }
            return false;
        }

        public static void DeleteIfExist(string directory, bool recursive = false)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive);
            }
        }

        public static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static List<FileInfo> RecursiveGetFileInfos(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var fileInfos = directoryInfo.GetFiles().ToList();

            var subDirs = Directory.GetDirectories(path);

            foreach (var subDir in subDirs)
            {
                var subFiles = RecursiveGetFileInfos(subDir);
                fileInfos.AddRange(subFiles);
            }
            return fileInfos;
        }


        public static bool IsSamePathRoot(string path1, string path2)
        {
            return Path.GetPathRoot(path1).Equals(Path.GetPathRoot(path2), StringComparison.OrdinalIgnoreCase);
        }

    }
}