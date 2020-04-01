namespace Spool.Utility
{
    /// <summary>扩展方法
    /// </summary>
    public static class Extensions
    {

        /// <summary>获取文件信息的Hash值
        /// </summary>
        public static string GenerateCode(this SpoolFile file)
        {
            return SHAUtil.GetHex16StringSHA1Hash($"{file.FilePoolName}{file.TrainIndex}{file.Path}");
        }
    }
}
