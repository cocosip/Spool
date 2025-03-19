namespace Spool
{
    /// <summary>
    /// 文件名称生成器
    /// </summary>
    public interface IFileNameGenerator
    {
        /// <summary>
        /// Generate fileName
        /// </summary>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        string GenerateFileName(string fileExt);
    }
}
