using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spool.Group;
using Spool.Utility;
using System;
using System.IO;
using Xunit;

namespace Spool.Tests.Group
{
    public class TrainManagerTest
    {
        private readonly IServiceProvider _provider;

        public TrainManagerTest()
        {
            IServiceCollection services = new ServiceCollection();
            services
                .AddLogging()
                .AddSpool();
            _provider = services.BuildServiceProvider();
        }

        [Theory]
        [InlineData("_00000001_", 1)]
        [InlineData("_x0002", 0)]
        [InlineData("_6003_", 6003)]
        public void GetTrainIndex_Test(string name, int? expected)
        {
            var descriptor = new GroupPoolDescriptor()
            {
                GroupName = "Group1",
                GroupPath = @"D\\Group1"
            };

            ITrainManager trainManager = new TrainManager(_provider, _provider.GetService<ILogger<TrainManager>>(), descriptor);

            Assert.Equal(expected, trainManager.GetTrainIndex(name));
        }


        [Theory]
        [InlineData("_000001_", true)]
        [InlineData("_230000_", true)]
        [InlineData("_000002", false)]
        [InlineData("_x3_", false)]
        public void IsTrainName_Test(string name, bool expected)
        {
            var descriptor = new GroupPoolDescriptor()
            {
                GroupName = "Group1",
                GroupPath = @"D:\\Group1"
            };

            ITrainManager trainManager = new TrainManager(_provider, _provider.GetService<ILogger<TrainManager>>(), descriptor);

            Assert.Equal(expected, trainManager.IsTrainName(name));
        }

        [Fact]
        public void FindTrains_Test()
        {
            var descriptor = new GroupPoolDescriptor()
            {
                GroupName = "Group1",
                GroupPath = Path.Combine(AppContext.BaseDirectory, "Group1")
            };

            DirectoryHelper.CreateIfNotExists(descriptor.GroupPath);
            ITrainManager trainManager = new TrainManager(_provider, _provider.GetService<ILogger<TrainManager>>(), descriptor);

            var train1Path = Path.Combine(descriptor.GroupPath, "_000001_");
            var train2Path = Path.Combine(descriptor.GroupPath, "_000002_");

            DirectoryHelper.CreateIfNotExists(train1Path);
            DirectoryHelper.CreateIfNotExists(train2Path);

            var trains = trainManager.FindTrains();
            Assert.Equal(2, trains.Count);

            if (Directory.Exists(descriptor.GroupPath))
            {
                Directory.Delete(descriptor.GroupPath, true);
            }
        }


    }
}
