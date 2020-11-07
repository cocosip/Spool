using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Spool
{
    /// <summary>
    /// SpoolFile extensions
    /// </summary>
    public static class SpoolFileExtensions
    {
        /// <summary>
        /// Get 'SpoolFile' hash code
        /// </summary>
        public static string GenerateCode(this SpoolFile file)
        {
            var source = $"{file.FilePool}{file.TrainIndex}{file.Path}";
            var sourceBytes = Encoding.UTF8.GetBytes(source);
            using (var sha1 = SHA1.Create())
            {
                var hashBuffer = sha1.ComputeHash(sourceBytes);
                return hashBuffer.Aggregate("", (current, b) => current + b.ToString("X2"));
            }
        }
    }
}
