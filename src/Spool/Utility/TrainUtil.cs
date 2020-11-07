using System.IO;
using System.Text.RegularExpressions;

namespace Spool.Utility
{
    /// <summary>
    /// Train util
    /// </summary>
    public static class TrainUtil
    {
        /// <summary>
        /// Generate train name by train index
        /// </summary>
        /// <param name="index">train index</param>
        /// <returns></returns>
        public static string GenerateTrainName(int index)
        {
            return $"_{index.ToString().PadLeft(6, '0')}_";
        }

        /// <summary>Generate train path by file pool path and train name
        /// </summary>
        /// <param name="path">File pool path</param>
        /// <param name="trainName">train name</param>
        /// <returns></returns>
        public static string GenerateTrainPath(string path, string trainName)
        {
            return Path.Combine(path, trainName);
        }

        /// <summary>
        /// Whether it is a train name
        /// </summary>
        /// <param name="name">train name</param>
        /// <returns></returns>
        public static bool IsTrainName(string name)
        {
            return Regex.IsMatch(name, @"^_[\d]{6}_$");
        }


        /// <summary>
        /// Get train index by train name
        /// </summary>
        /// <param name="name">train name</param>
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
