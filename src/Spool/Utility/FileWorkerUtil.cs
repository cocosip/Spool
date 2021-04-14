using System.IO;
using System.Text.RegularExpressions;

namespace Spool.Utility
{
    /// <summary>
    /// FileWorker工具类
    /// </summary>
    public static class FileWorkerUtil
    {

        /// <summary>
        /// 根据序号,生成名称
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GenerateName(int index)
        {
            return $"_{index.ToString().PadLeft(6, '0')}_";
        }

        /// <summary>
        /// 生成存储路径(根据存储的FilePool路径,以及当前FileWorker的名称)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GeneratePath(string path, string name)
        {
            return Path.Combine(path, name);
        }

        /// <summary>
        /// 判断是否为FileWorker的名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsFileWorkerName(string name)
        {
            return Regex.IsMatch(name, @"^_[\d]{6}_$");
        }

        /// <summary>
        /// 根据名称获取Index
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetIndex(string name)
        {
            if (int.TryParse(name.Replace('_', ' '), out int r))
            {
                return r;
            }
            return 0;
        }

    }
}
