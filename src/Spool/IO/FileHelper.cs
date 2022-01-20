using System;
using System.IO;

namespace Spool.IO
{
    internal static class FileHelper
    {
        internal static void DeleteIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }


       
    }
}