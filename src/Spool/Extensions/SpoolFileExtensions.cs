using Spool.Utility;

namespace Spool.Extensions
{
    /// <summary>SpoolFile扩展
    /// </summary>
    public static class SpoolFileExtensions
    {
        /// <summary>获取文件信息的Hash值
        /// </summary>
        public static string GenerateCode(this SpoolFile file)
        {
            //return file.Path;
            return SHAUtil.GetHex16StringSHA1Hash($"{file.FilePoolName}{file.TrainIndex}{file.Path}");
        }
    }
}
