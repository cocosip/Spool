using Spool.Utility;
using Xunit;

namespace Spool.Tests.Utility
{
    public class PathUtilTest
    {
        [Theory]
        [InlineData("D:\\Test\\Test1", "")]
        [InlineData("D:\\Path\\t1.dcm", ".dcm")]
        [InlineData("Path\\Test1\\f1.jpg", ".jpg")]
        [InlineData("C:\\sss 111.PNG", ".PNG")]
        public void GetPathExtension_Test(string path, string expected)
        {
            Assert.Equal(expected, PathUtil.GetPathExtension(path));
        }
    }
}
