using System.IO;

namespace Spool.IO
{
    internal static class DirectoryHelper
    {
        internal static void CreateIfNotExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        internal static void DeleteIfExist(string directory, bool recursive = false)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive);
            }
        }

        internal static void DirectoryCopy(string sourceDir, string targetDir)
        {
            CreateIfNotExists(targetDir);
            DirectoryInfo dir = new(sourceDir);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)
                {
                    DirectoryCopy(i.FullName, Path.Combine(targetDir, i.Name));
                }
                else
                {
                    File.Copy(i.FullName, Path.Combine(targetDir, i.Name), true);
                }
            }

        }

    }
}