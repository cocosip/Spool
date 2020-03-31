using System.IO;
using System.Text.RegularExpressions;

namespace Spool.Utility
{
    /// <summary>序列名称
    /// </summary>
    public static class TrainUtil
    {
        /// <summary>
        /// 根据索引号生成序列名称
        /// </summary>
        /// <param name="index">索引号</param>
        /// <returns></returns>
        public static string GenerateTrainName(int index)
        {
            return $"_{index.ToString().PadLeft(6, '0')}_";
        }

        /// <summary>根据文件池路径,序列名生成序列路径
        /// </summary>
        /// <param name="path">文件池路径</param>
        /// <param name="trainName">序列名</param>
        /// <returns></returns>
        public static string GenerateTrainPath(string path,string trainName)
        {
            return Path.Combine(path, trainName);
        }


        /// <summary>
        /// 判断是否为有效的序列名称
        /// </summary>
        /// <param name="name">序列名</param>
        /// <returns></returns>
        public static bool IsTrainName(string name)
        {
            return Regex.IsMatch(name, @"^_[\d]{6}_$");
        }

        /// <summary>
        /// 根据序列名称获取序列索引
        /// </summary>
        /// <param name="name">序列名称</param>
        /// <returns></returns>
        public static int GetTrainIndex(string name)
        {
            if (int.TryParse(name.Replace('_', ' '), out int r))
            {
                return r;
            }
            return 0;
        }


    }
}
