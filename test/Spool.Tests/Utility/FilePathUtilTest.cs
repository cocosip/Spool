using Spool.Utility;
using System;
using System.IO;
using Xunit;

namespace Spool.Tests.Utility
{
    public class FilePathUtilTest
    {
        [Theory]
        [InlineData(@"C:\A\B.txt", ".txt")]
        [InlineData(@"\BB.jpg", ".jpg")]
        [InlineData(@"..jpg", ".jpg")]
        [InlineData(@"D:\A\B\C", "")]
        [InlineData(@"", "")]
        public void GetPathExtension_Test(string path, string expected)
        {
            var actual = FilePathUtil.GetPathExtension(path);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void CreateIfNotExists_Test()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test01\\");

            FilePathUtil.CreateIfNotExists(path);
            Assert.True(Directory.Exists(path));
            FilePathUtil.CreateIfNotExists(path);

            FilePathUtil.DeleteDirIfExist(path, false);
            Assert.False(Directory.Exists(path));


        }

        [Fact]
        public void DeleteIfExist()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test02\\");
            FilePathUtil.CreateIfNotExists(path);

            var filePath = Path.Combine(path, "1.text");
            File.WriteAllText(filePath, "haha");
            Assert.True(File.Exists(filePath));
            Assert.True(FilePathUtil.DeleteFileIfExists(filePath));
            Assert.False(FilePathUtil.DeleteFileIfExists(filePath));
            Assert.False(File.Exists(filePath));

            Directory.Delete(path, true);
        }



    }
}
