using System.IO;

namespace Spool.Utility
{
    /// <summary>DirectoryHelper
    /// </summary>
    public static class DirectoryHelper
    {
        /// <summary>Create the directory if not exist
        /// </summary>
        public static bool CreateIfNotExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                return true;
            }
            return false;
        }

        /// <summary>如果文件夹存在就删除
        /// </summary>
        public static bool DeleteIfExist(string directory, bool recursive = false)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive);
                return true;
            }
            return false;
        }

        /// <summary>Copy files and directorys
        /// </summary>
        public static void DirectoryCopy(string sourceDir, string targetDir)
        {

            CreateIfNotExists(targetDir);
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
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
