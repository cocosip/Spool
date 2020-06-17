using Spool.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Spool.Utility
{
    /// <summary>Path Util
    /// </summary>
    internal static class PathUtil
    {
        /// <summary>获取某个路径,或者文件中的文件扩展名
        /// </summary>
        /// <param name="path">路径或者文件地址</param>
        /// <returns></returns>
        internal static string GetPathExtension(string path)
        {
            if (!path.IsNullOrWhiteSpace() && path.IndexOf('.') >= 0)
            {
                return path.Substring(path.LastIndexOf('.'));
            }
            return "";
        }
    }
}
