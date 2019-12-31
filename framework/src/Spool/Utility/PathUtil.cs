namespace Spool.Utility
{
    /// <summary>Path Util
    /// </summary>
    public static class PathUtil
    {
        /// <summary>Get path extension
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns></returns>
        public static string GetPathExtension(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && path.IndexOf('.') >= 0)
            {
                return path.Substring(path.LastIndexOf('.'));
            }
            return "";
        }

    }
}
