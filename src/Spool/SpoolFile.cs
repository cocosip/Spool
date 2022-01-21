using System;
using System.IO;

namespace Spool
{
    public class SpoolFile : IEquatable<SpoolFile>
    {
        /// <summary>
        /// 文件池名称
        /// </summary>
        /// <value></value>
        public string FilePool { get; set; }

        /// <summary>
        /// Worker序号
        /// </summary>
        /// <value></value>
        public int WorkerNumber { get; set; }

        /// <summary>
        /// 文件存储路径
        /// </summary>
        /// <value></value>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        /// <value></value>
        public string Ext
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FilePath))
                {
                    return Path.GetExtension(FilePath);
                }
                return string.Empty;
            }
        }

        public SpoolFile(string filePool, int workerNumber) : this(filePool, workerNumber, "")
        {

        }

        public SpoolFile(string filePool, int workerNumber, string filePath)
        {
            FilePool = filePool;
            WorkerNumber = workerNumber;
            FilePath = filePath;
        }

        public bool Equals(SpoolFile other)
        {

            if (other == null)
            {
                return false;
            }

            return FilePool.Equals(other.FilePool, StringComparison.OrdinalIgnoreCase) && WorkerNumber == other.WorkerNumber && FilePath.Equals(other.FilePath, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return StringComparer.InvariantCulture.GetHashCode(FilePool) | StringComparer.InvariantCulture.GetHashCode(FilePath) | WorkerNumber.GetHashCode();
        }

    }
}