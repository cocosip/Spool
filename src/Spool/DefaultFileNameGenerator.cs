using Spool.Utility;

namespace Spool
{
    /// <summary>
    /// 文件名称生成器 
    /// </summary>
    public class DefaultFileNameGenerator : IFileNameGenerator
    {
        /// <summary>
        /// Generate fileName
        /// </summary>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public virtual string GenerateFileName(string fileExt)
        {
            var fileName = $"{ObjectId.GenerateNewStringId()}{fileExt}";
            return fileName;
        }
    }
}
