using Microsoft.Extensions.Logging;
using Spool.Group;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Spool.Tests.Group
{
    public class TrainManagerTest
    {
        [Theory]
        [InlineData("_00001_", 1)]
        [InlineData("_x0002", 0)]
        [InlineData("_6003_", 6003)]
        public void GetTrainIndex_Test(string name, int? expected)
        {
            var descriptor = new GroupPoolDescriptor()
            {
                GroupName = "Group1",
                GroupPath = @"D\\Group1"
            };

            ITrainManager trainManager = new TrainManager(LoggerHelper.GetLogger<TrainManager>(), descriptor);

            Assert.Equal(expected, trainManager.GetTrainIndex(name));
        }
    }
}
