using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spool.IO
{
    internal static class PathUtil
    {
        internal static List<FileInfo> RecursiveGetFileInfos(string path)
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


        internal static bool IsSamePathRoot(string path1, string path2)
        {
            return Path.GetPathRoot(path1).Equals(Path.GetPathRoot(path2), StringComparison.OrdinalIgnoreCase);
        }
    }
}