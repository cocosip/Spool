using Spool.Utility;
using System;
using Xunit;

namespace Spool.Tests.Utility
{
    public class IdGeneratorTest
    {

        [Fact]
        public void GenerateId_Test()
        {
            var idGenerator = new IdGenerator();
            long id1 = idGenerator.GenerateId();
            long id2 = idGenerator.GenerateId();
            Assert.True(id2 > id1);

            var id3 = idGenerator.GenerateIdAsString();
            Assert.True(Convert.ToInt64(id3) > id2);

        }
    }
}
