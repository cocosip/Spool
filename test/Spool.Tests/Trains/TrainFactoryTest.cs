using Microsoft.Extensions.Logging;
using Moq;
using Spool.Trains;
using Spool.Utility;
using Spool.Writers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Spool.Tests.Trains
{
    public class TrainFactoryTest
    {
        private readonly Mock<ILogger<TrainFactory>> _mockLogger;
        public TrainFactoryTest()
        {
            _mockLogger = new Mock<ILogger<TrainFactory>>();
        }

        [Fact]
        public void Initialize_Test()
        {
            FilePoolOption filePoolOption1 = new FilePoolOption();

            var mockTrain = new Mock<ITrain>();
            mockTrain.Setup(x => x.Index)
                .Returns(1);

            var mockTrainBuilder = new Mock<ITrainBuilder>();
            mockTrainBuilder.Setup(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()))
                .Returns(new List<ITrain>());

            mockTrainBuilder.Setup(x => x.BuildTrain(1, It.IsAny<FilePoolOption>()))
                .Returns(mockTrain.Object);


            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption1, mockTrainBuilder.Object);

            trainFactory.Initialize();
            //第二次
            trainFactory.Initialize();

            mockTrainBuilder.Verify(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()), Times.Once);
        }

        [Fact]
        public void BuildInfo_Test()
        {
            FilePoolOption filePoolOption1 = new FilePoolOption()
            {
                Name = "Pool1",
                Path = "D:\\Pool1"
            };

            var mockTrain = new Mock<ITrain>();
            mockTrain.Setup(x => x.Index)
                .Returns(1);
            mockTrain.Setup(x => x.TrainType)
               .Returns(TrainType.ReadWrite);
            mockTrain.Setup(x => x.Path)
            .Returns("D:\\Pool1\\_000001_");
            mockTrain.Setup(x => x.Name)
                .Returns("_000001_");

            var mockTrainBuilder = new Mock<ITrainBuilder>();

            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption1, mockTrainBuilder.Object);

            var trainInfo = trainFactory.BuildInfo(mockTrain.Object);

            Assert.Equal("Pool1", trainInfo.FilePoolName);
            Assert.Equal("D:\\Pool1", trainInfo.FilePoolPath);
            Assert.Equal(1, trainInfo.Index);
            Assert.Equal("_000001_", trainInfo.Name);
            Assert.Equal("D:\\Pool1\\_000001_", trainInfo.Path);
            Assert.Equal(TrainType.ReadWrite, trainInfo.TrainType);
        }

        [Fact]
        public void GetWriteTrain_ReadFromDict_Test()
        {
            FilePoolOption filePoolOption = new FilePoolOption()
            {
                Path = "D:\\Pool1"
            };
            TrainOption trainOption = new TrainOption()
            {
                Index = 1
            };
            var mockTrainLogger = new Mock<ILogger<Train>>();
            var mockIdGenerator = new Mock<IdGenerator>();
            var mockFileWriterPool = new Mock<IFileWriterPool>();

            ITrain train = new Train(mockTrainLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, trainOption);


            var mockTrainBuilder = new Mock<ITrainBuilder>();
            mockTrainBuilder.Setup(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()))
                .Returns(new List<ITrain>());

            mockTrainBuilder.Setup(x => x.BuildTrain(1, It.IsAny<FilePoolOption>()))
                .Returns(train);


            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption, mockTrainBuilder.Object);

            trainFactory.Initialize();

            var writeTrain = trainFactory.GetWriteTrain();

            Assert.Equal(1, writeTrain.Index);


        }

    }
}
