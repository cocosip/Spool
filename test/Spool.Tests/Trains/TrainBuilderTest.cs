using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Spool.Trains;
using Spool.Utility;
using System;
using System.IO;
using Xunit;

namespace Spool.Tests.Trains
{
    public class TrainBuilderTest
    {
        private readonly Mock<ILogger<TrainBuilder>> _mockLogger;
        public TrainBuilderTest()
        {
            _mockLogger = new Mock<ILogger<TrainBuilder>>();
        }

        [Fact]
        public void BuildTrain_Test()
        {
            var trainIndex = 2;

            var filePoolOption1 = new FilePoolOption();
            var trainOption1 = new TrainOption();

            var mockTrain = new Mock<ITrain>();
            mockTrain.Setup(x => x.Index)
                .Returns(trainIndex);

            var mockScopeServiceProvider = new Mock<IServiceProvider>();
            mockScopeServiceProvider.Setup(x => x.GetService(typeof(FilePoolOption)))
                .Returns(filePoolOption1);

            mockScopeServiceProvider.Setup(x => x.GetService(typeof(TrainOption)))
                 .Returns(trainOption1);

            mockScopeServiceProvider.Setup(x => x.GetService(typeof(ITrain)))
               .Returns(mockTrain.Object);

            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(x => x.ServiceProvider)
                .Returns(mockScopeServiceProvider.Object);

            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceScopeFactory.Setup(x => x.CreateScope())
                .Returns(mockScope.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockServiceScopeFactory.Object);

            var filePoolOption2 = new FilePoolOption()
            {
                Name = "Pool2",
                Path = "D:\\Pool2",
                MaxFileWriterCount = 300,
                ConcurrentFileWriterCount = 5,
                WriteBufferSize = 2048,
                TrainMaxFileCount = 1000,
                EnableFileWatcher = true,
                FileWatcherPath = "D:\\Pool2Watcher",
                ScanFileWatcherMillSeconds = 2000,
                EnableAutoReturn = true,
                ScanReturnFileMillSeconds = 1000,
                AutoReturnSeconds = 300
            };
            ITrainBuilder trainBuilder = new TrainBuilder(_mockLogger.Object, mockServiceProvider.Object);
            var train = trainBuilder.BuildTrain(trainIndex, filePoolOption2);
            Assert.Equal(trainIndex, train.Index);
            Assert.Equal(trainIndex, trainOption1.Index);

            Assert.Equal("Pool2", filePoolOption1.Name);
            Assert.Equal("D:\\Pool2", filePoolOption1.Path);
            Assert.Equal(300, filePoolOption1.MaxFileWriterCount);
            Assert.Equal(5, filePoolOption1.ConcurrentFileWriterCount);
            Assert.Equal(2048, filePoolOption1.WriteBufferSize);
            Assert.True(filePoolOption1.EnableFileWatcher);
            Assert.Equal("D:\\Pool2Watcher", filePoolOption1.FileWatcherPath);
            Assert.Equal(2000, filePoolOption1.ScanFileWatcherMillSeconds);
            Assert.True(filePoolOption1.EnableAutoReturn);
            Assert.Equal(1000, filePoolOption1.ScanReturnFileMillSeconds);
            Assert.Equal(300, filePoolOption1.AutoReturnSeconds);

            mockScopeServiceProvider.Verify(x => x.GetService(typeof(ITrain)), Times.Once);
        }


        [Fact]
        public void BuildPoolTrains_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool1",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool1")
            };
            var filePoolOption1 = new FilePoolOption();
            var trainOption1 = new TrainOption();
            
            var mockTrain = new Mock<ITrain>();

            var mockScopeServiceProvider = new Mock<IServiceProvider>();
            mockScopeServiceProvider.Setup(x => x.GetService(typeof(FilePoolOption)))
                .Returns(filePoolOption1);

            mockScopeServiceProvider.Setup(x => x.GetService(typeof(TrainOption)))
                 .Returns(trainOption1);

            mockScopeServiceProvider.Setup(x => x.GetService(typeof(ITrain)))
               .Returns(mockTrain.Object);

            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(x => x.ServiceProvider)
                .Returns(mockScopeServiceProvider.Object);

            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceScopeFactory.Setup(x => x.CreateScope())
                .Returns(mockScope.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockServiceScopeFactory.Object);
            ITrainBuilder trainBuilder = new TrainBuilder(_mockLogger.Object, mockServiceProvider.Object);

            var train1Path = Path.Combine(filePoolOption.Path, "_000001_");
            var train2Path = Path.Combine(filePoolOption.Path, "_000002_");
            FilePathUtil.CreateIfNotExists(train1Path);
            FilePathUtil.CreateIfNotExists(train2Path);

            var trains = trainBuilder.BuildPoolTrains(filePoolOption);

            Assert.Equal(2, trains.Count);
            Assert.Equal("Pool1", filePoolOption1.Name);
            Assert.Equal(filePoolOption.Path, filePoolOption1.Path);

            FilePathUtil.DeleteDirIfExist(filePoolOption.Path, true);
            Assert.False(Directory.Exists(filePoolOption.Path));

        }

    }
}
