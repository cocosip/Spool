using Spool.Extensions;
using Xunit;

namespace Spool.Tests.Extensions
{
    public class SpoolFileExtensionsTest
    {
        [Fact]
        public void GenerateCode_Test()
        {
            var spoolFile1 = new SpoolFile()
            {
                FilePoolName = "Pool1",
                TrainIndex = 1,
                Path = "D:\\File1.dcm",
            };

            var spoolFile2 = new SpoolFile()
            {
                FilePoolName = "Pool1",
                TrainIndex = 1,
                Path = "D:\\File1.dcm",
            };

            var spoolFile3 = new SpoolFile()
            {
                FilePoolName = "Pool1",
                TrainIndex = 2,
                Path = "D:\\File1.dcm",
            };

            var spoolFile4 = new SpoolFile()
            {
                FilePoolName = "Pool4",
                TrainIndex = 2,
                Path = "D:\\File1.dcm",
            };

            var spoolFile5 = new SpoolFile()
            {
                FilePoolName = "Pool4",
                TrainIndex = 2,
                Path = "D:\\File5.dcm",
            };

            var code1 = spoolFile1.GenerateCode();
            var code2 = spoolFile2.GenerateCode();
            var code3 = spoolFile3.GenerateCode();
            var code4 = spoolFile4.GenerateCode();
            var code5 = spoolFile5.GenerateCode();
            Assert.Equal(code1, code2);
            Assert.NotEqual(code2, code3);
            Assert.NotEqual(code3, code4);
            Assert.NotEqual(code4, code5);
        }

    }
}
