using Spool.Utility;
using System.Runtime.InteropServices;
using Xunit;

namespace Spool.Tests.Utility
{
    public class TrainUtilTest
    {
        [Theory]
        [InlineData("_00000001_", 1)]
        [InlineData("_x0002", 0)]
        [InlineData("_6003_", 6003)]
        public void GetTrainIndex_Test(string name, int? expected)
        {
            Assert.Equal(expected, TrainUtil.GetTrainIndex(name));
        }

        [Theory]
        [InlineData("_000001_", true)]
        [InlineData("_230000_", true)]
        [InlineData("_000002", false)]
        [InlineData("_x3_", false)]
        public void IsTrainName_Test(string name, bool expected)
        {
            Assert.Equal(expected, TrainUtil.IsTrainName(name));
        }

        [Theory]
        [InlineData(1, "_000001_")]
        [InlineData(230000, "_230000_")]
        [InlineData(2, "_000002_")]
        public void GenerateTrainName_Test(int index, string expected)
        {
            Assert.Equal(expected, TrainUtil.GenerateTrainName(index));
        }

        [Fact]
        public void GenerateTrainPath_Test()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var path1 = TrainUtil.GenerateTrainPath("D:\\Test1", "_000001_");
                Assert.Equal("D:\\Test1\\_000001_", path1);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var path1 = TrainUtil.GenerateTrainPath("/usr/local/", "_000001_");
                Assert.Equal("/usr/local/_000001_", path1);
            }
        }
    }
}
