using System.IO;

namespace Spool.Utility
{
    /// <summary>
    /// FileHelper
    /// </summary>
    public static class FileHelper
    {

        /// <summary>
        /// Delete file if exist
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
