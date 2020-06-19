using Microsoft.Extensions.Logging;
using Moq;
using Spool.Trains;
using Spool.Utility;
using Spool.Writers;
using System;
using System.Collections.Generic;
using System.IO;
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
        public void Initialize_TwoTrain_Test()
        {

            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool2",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool2")
            };

            var trains = new List<ITrain>();
            var mockTrain1 = new Mock<ITrain>();
            mockTrain1.Setup(x => x.Index)
                .Returns(1);

            var mockTrain2 = new Mock<ITrain>();
            mockTrain2.Setup(x => x.Index)
                .Returns(2);

            trains.Add(mockTrain1.Object);
            trains.Add(mockTrain2.Object);

            var mockTrainBuilder = new Mock<ITrainBuilder>();
            mockTrainBuilder.Setup(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()))
                .Returns(trains);

            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption, mockTrainBuilder.Object);

            trainFactory.Initialize();

            mockTrain1.Verify(x => x.ChangeType(TrainType.Read), Times.Once);
            mockTrain2.Verify(x => x.ChangeType(TrainType.Write), Times.Once);

            var getTrains = trainFactory.GetTrains(x => true);
            Assert.Equal(2, getTrains.Count);
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
        public void GetWriteTrain_FromDict_Test()
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
            Assert.Equal("_000001_", writeTrain.Name);
            Assert.Equal("D:\\Pool1\\_000001_", writeTrain.Path);
            Assert.Equal(TrainType.ReadWrite, writeTrain.TrainType);

            mockTrainBuilder.Verify(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()), Times.Once);
            mockTrainBuilder.Verify(x => x.BuildTrain(It.IsAny<int>(), It.IsAny<FilePoolOption>()), Times.Once);

        }

        [Fact]
        public void GetWriteTrain_BuildNew_Test()
        {
            FilePoolOption filePoolOption = new FilePoolOption()
            {
                Path = "D:\\Pool1"
            };
            TrainOption trainOption = new TrainOption()
            {
                Index = 2
            };
            var mockTrainLogger = new Mock<ILogger<Train>>();
            var mockIdGenerator = new Mock<IdGenerator>();
            var mockFileWriterPool = new Mock<IFileWriterPool>();
            var mockTrain = new Mock<ITrain>();
            mockTrain.SetupGet(x => x.Index)
                .Returns(1);
            mockTrain.SetupGet(x => x.TrainType)
                .Returns(TrainType.Read);


            ITrain train = new Train(mockTrainLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, trainOption);

            var mockTrainBuilder = new Mock<ITrainBuilder>();
            mockTrainBuilder.Setup(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()))
                .Returns(new List<ITrain>());

            mockTrainBuilder.Setup(x => x.BuildTrain(1, It.IsAny<FilePoolOption>()))
                .Returns(mockTrain.Object);

            mockTrainBuilder.Setup(x => x.BuildTrain(2, It.IsAny<FilePoolOption>()))
                .Returns(train);

            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption, mockTrainBuilder.Object);
            trainFactory.Initialize();

            var writeTrain = trainFactory.GetWriteTrain();
            Assert.Equal(2, writeTrain.Index);

            mockTrainBuilder.Verify(x => x.BuildTrain(It.IsAny<int>(), It.IsAny<FilePoolOption>()), Times.Between(1, 3, Moq.Range.Exclusive));

        }

        [Fact]
        public void GetReadTrain_FromDict_Test()
        {
            var filePoolOption = new FilePoolOption();
            var mockTrain = new Mock<ITrain>();
            mockTrain.Setup(x => x.Index).Returns(1);
            mockTrain.Setup(x => x.TrainType).Returns(TrainType.Read);
            mockTrain.Setup(x => x.IsEmpty()).Returns(false);

            var mockTrainBuilder = new Mock<ITrainBuilder>();
            mockTrainBuilder.Setup(x => x.BuildTrain(It.IsAny<int>(), It.IsAny<FilePoolOption>()))
                .Returns(mockTrain.Object);
            mockTrainBuilder.Setup(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()))
                .Returns(new List<ITrain>());

            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption, mockTrainBuilder.Object);
            trainFactory.Initialize();

            var train = trainFactory.GetReadTrain();
            Assert.Equal(1, train.Index);
            Assert.Equal(TrainType.Read, train.TrainType);

            var trains = trainFactory.GetTrains(x => true);
            Assert.Single(trains);

            mockTrainBuilder.Verify(x => x.BuildTrain(It.IsAny<int>(), It.IsAny<FilePoolOption>()), Times.Once);
        }

        [Fact]
        public void GetReadTrain_Default_Test()
        {

            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool3",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool3")
            };


            var mockTrainLogger = new Mock<ILogger<Train>>();
            var mockIdGenerator = new Mock<IdGenerator>();
            var mockFileWriterPool = new Mock<IFileWriterPool>();
            ITrain train = new Train(mockTrainLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, new TrainOption() { Index = 3 });

            Assert.Equal(TrainType.Default, train.TrainType);

            var mockTrain1 = new Mock<ITrain>();
            mockTrain1.Setup(x => x.Index).Returns(2);
            mockTrain1.Setup(x => x.TrainType).Returns(TrainType.Write);
            var mockTrain2 = new Mock<ITrain>();
            mockTrain2.Setup(x => x.Index).Returns(4);
            mockTrain2.Setup(x => x.TrainType).Returns(TrainType.Write);

            var mockTrainBuilder = new Mock<ITrainBuilder>();
            //mockTrainBuilder.Setup(x => x.BuildTrain(It.IsAny<int>(), It.IsAny<FilePoolOption>()))
            //    .Returns(train);
            mockTrainBuilder.Setup(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()))
                .Returns(new List<ITrain>()
                {
                    mockTrain1.Object,
                    mockTrain2.Object,
                    train
                });

            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption, mockTrainBuilder.Object);
            trainFactory.Initialize();

            var readTrain = trainFactory.GetReadTrain();
            Assert.Equal(3, readTrain.Index);
            Assert.Equal(TrainType.Read, train.TrainType);

        }

        [Fact]
        public void GetReadTrain_ReadWrite_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool3",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool3")
            };

            var mockTrainLogger = new Mock<ILogger<Train>>();
            var mockIdGenerator = new Mock<IdGenerator>();
            var mockFileWriterPool = new Mock<IFileWriterPool>();
            ITrain train = new Train(mockTrainLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, new TrainOption() { Index = 3 });

            Assert.Equal(TrainType.Default, train.TrainType);

            var mockTrainBuilder = new Mock<ITrainBuilder>();
            mockTrainBuilder.Setup(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()))
                .Returns(new List<ITrain>());
            mockTrainBuilder.Setup(x => x.BuildTrain(It.IsAny<int>(), It.IsAny<FilePoolOption>()))
                .Returns(train);

            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption, mockTrainBuilder.Object);
            trainFactory.Initialize();

            var readTrain = trainFactory.GetReadTrain();
            Assert.Equal(3, readTrain.Index);
            Assert.Equal(TrainType.ReadWrite, readTrain.TrainType);
            mockTrainBuilder.Verify(x => x.BuildTrain(It.IsAny<int>(), It.IsAny<FilePoolOption>()), Times.Once);
        }

        [Fact]
        public void GetReadTrain_Write_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool3",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool3")
            };

            var mockTrainLogger = new Mock<ILogger<Train>>();
            var mockIdGenerator = new Mock<IdGenerator>();
            var mockFileWriterPool = new Mock<IFileWriterPool>();
            ITrain train = new Train(mockTrainLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, new TrainOption() { Index = 3 });
            //train.ChangeType(TrainType.Write);

            Assert.Equal(TrainType.Default, train.TrainType);

            //var mockTrain1 = new Mock<ITrain>();
            //mockTrain1.Setup(x => x.Index).Returns(1);
            //mockTrain1.Setup(x => x.TrainType).Returns(TrainType.Write);

            //var mockTrain2 = new Mock<ITrain>();
            //mockTrain1.Setup(x => x.Index).Returns(1);
            //mockTrain1.Setup(x => x.TrainType).Returns(TrainType.Write);

            var mockTrainBuilder = new Mock<ITrainBuilder>();
            mockTrainBuilder.Setup(x => x.BuildPoolTrains(It.IsAny<FilePoolOption>()))
                .Returns(new List<ITrain>());
            mockTrainBuilder.Setup(x => x.BuildTrain(It.IsAny<int>(), It.IsAny<FilePoolOption>()))
                .Returns(train);

            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption, mockTrainBuilder.Object);
            trainFactory.Initialize();

            //类型转换
            train.ChangeType(TrainType.Write);

            var readTrain = trainFactory.GetReadTrain();
            Assert.Equal(3, readTrain.Index);
            Assert.Equal(TrainType.ReadWrite, readTrain.TrainType);
        }

        [Fact]
        public void GetReadTrain_TrainNullException_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool3",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool3")
            };
            var mockTrainBuilder = new Mock<ITrainBuilder>();
            ITrainFactory trainFactory = new TrainFactory(_mockLogger.Object, filePoolOption, mockTrainBuilder.Object);
            Assert.Throws<ArgumentException>(() =>
            {
                var readTrain = trainFactory.GetReadTrain();
            });
        }



        [Fact]
        public void GetTrainByIndex_Test()
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

            var train1 = trainFactory.GetTrainByIndex(1);
            Assert.Equal(1, train1.Index);

            var train2 = trainFactory.GetTrainByIndex(2);
            Assert.Null(train2);
        }
    }
}
