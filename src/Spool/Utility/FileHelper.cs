using System.IO;

namespace Spool.Utility
{
    /// <summary>FileHelper
    /// </summary>
    internal static class FileHelper
    {
        /// <summary>If file exist,delete the file.
        /// </summary>
        internal static bool DeleteIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
    }
}
