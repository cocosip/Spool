using Spool.Tests.TestObjects;
using Xunit;

namespace Spool.Tests
{
    public class FilePoolNameAttributeTest
    {
        [Fact]
        public void Should_Get_Specified_Name()
        {
            var name = FilePoolNameAttribute
                  .GetFilePoolName<TestFilePool2>();

            Assert.Equal("test-filepool2", name);
        }

        [Fact]
        public void Should_Get_Full_Class_Name_If_Not_Specified()
        {
            var expected = typeof(TestFilePool1).FullName;

            var name = FilePoolNameAttribute
                  .GetFilePoolName<TestFilePool1>();
            Assert.Equal(expected, name);
        }


        [Fact]
        public void GetName_By_Type()
        {
            var expected = typeof(TestFilePool3).FullName;
            var name = FilePoolNameAttribute
                .GetFilePoolName(typeof(TestFilePool3));
            Assert.Equal(expected, name);
        }
    }
}
