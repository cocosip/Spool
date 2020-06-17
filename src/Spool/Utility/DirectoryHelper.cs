using System.IO;

namespace Spool.Utility
{
    /// <summary>DirectoryHelper
    /// </summary>
    internal static class DirectoryHelper
    {
        /// <summary>Create the directory if not exist
        /// </summary>
        internal static bool CreateIfNotExists(string directory)
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
        internal static bool DeleteIfExist(string directory, bool recursive = false)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive);
                return true;
            }
            return false;
        }
    }
}
