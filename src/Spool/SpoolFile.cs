using Spool.Utility;
using System;

namespace Spool
{
    /// <summary>
    /// Spool file info
    /// </summary>
    public class SpoolFile : IEquatable<SpoolFile>
    {
        /// <summary>
        /// The name of file pool
        /// </summary>
        public string FilePool { get; set; }

        /// <summary>
        /// The index of train
        /// </summary>
        public int TrainIndex { get; set; }

        /// <summary>
        /// File path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// File extension
        /// </summary>
        public string FileExt
        {
            get
            {
                return FilePathUtil.GetPathExtension(Path);
            }
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public SpoolFile()
        {

        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="filePool"></param>
        /// <param name="trainIndex"></param>
        public SpoolFile(string filePool, int trainIndex) : this(filePool, trainIndex, string.Empty)
        {

        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="filePool"></param>
        /// <param name="trainIndex"></param>
        /// <param name="path"></param>
        public SpoolFile(string filePool, int trainIndex, string path)
        {
            FilePool = filePool;
            TrainIndex = trainIndex;
            Path = path;
        }

        /// <summary>
        /// Clone a 'SpoolFile'
        /// </summary>
        /// <returns></returns>
        public SpoolFile Clone()
        {
            return new SpoolFile(FilePool, TrainIndex, Path);
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SpoolFile other)
        {
            return FilePool == other.FilePool && TrainIndex == other.TrainIndex && Path == other.Path;
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is SpoolFile other && Equals(other);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return StringComparer.InvariantCulture.GetHashCode(FilePool) | StringComparer.InvariantCulture.GetHashCode(Path) | TrainIndex.GetHashCode();
        }

        /// <summary>
        /// SpoolFile == SpoolFile
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static bool operator ==(SpoolFile f1, SpoolFile f2) => f1.Equals(f2);

        /// <summary>
        /// SpoolFile != SpoolFile
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static bool operator !=(SpoolFile f1, SpoolFile f2) => !f1.Equals(f2);
    }
}
