﻿using System.IO;

namespace Spool.Utility
{
    /// <summary>Path Util
    /// </summary>
    public static class FilePathUtil
    {
        /// <summary>获取某个路径,或者文件中的文件扩展名
        /// </summary>
        /// <param name="path">路径或者文件地址</param>
        /// <returns></returns>
        public static string GetPathExtension(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && path.IndexOf('.') >= 0)
            {
                return path.Substring(path.LastIndexOf('.'));
            }
            return "";
        }


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
        public static bool DeleteDirIfExist(string directory, bool recursive = false)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive);
                return true;
            }
            return false;
        }

        /// <summary>If file exist,delete the file.
        /// </summary>
        public static bool DeleteFileIfExists(string filePath)
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