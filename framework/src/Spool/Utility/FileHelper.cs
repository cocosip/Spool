using System.IO;

namespace Spool.Utility
{
    /// <summary>FileHelper
    /// </summary>
    public static class FileHelper
    {
        /// <summary>If file exist,delete the file.
        /// </summary>
        public static bool DeleteIfExists(string filePath)
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
