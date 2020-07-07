using Microsoft.Extensions.Logging;
using Moq;
using Spool.Trains;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Spool.Tests
{
    public class FilePoolTest
    {
        private readonly Mock<ILogger<FilePool>> _mockLogger;
        public FilePoolTest()
        {
            _mockLogger = new Mock<ILogger<FilePool>>();
        }

        [Fact]
        public void Start_Shutdown_Test()
        {

            var mockTrainFactory = new Mock<ITrainFactory>();
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool_FPool1",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool_FPool1"),
                EnableAutoReturn = true,
                EnableFileWatcher = true,
                FileWatcherPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileWatcher_FPool1"),
            };
            IFilePool filePool = new FilePool(_mockLogger.Object, mockTrainFactory.Object, filePoolOption);


            filePool.Start();
            filePool.Start();

            Assert.True(Directory.Exists(filePoolOption.Path));
            Assert.True(Directory.Exists(filePoolOption.FileWatcherPath));
            Assert.True(filePool.IsRunning);

            mockTrainFactory.Verify(x => x.Initialize(), Times.Once);

            Assert.Equal(filePoolOption.Name, filePool.Option.Name);
            Assert.Equal(filePoolOption.Path, filePool.Option.Path);

            filePool.Shutdown();
            filePool.Shutdown();
            Assert.False(filePool.IsRunning);

            Directory.Delete(filePoolOption.Path, true);
            Directory.Delete(filePoolOption.FileWatcherPath, true);
        }

        [Fact]
        public async Task WriteFileAsync_Test()
        {
            var mockTrain = new Mock<ITrain>();
            mockTrain.Setup(x => x.PendingCount).Returns(2);

            var mockTrainFactory = new Mock<ITrainFactory>();
            mockTrainFactory.Setup(x => x.GetWriteTrain())
                .Returns(mockTrain.Object);
            mockTrainFactory.Setup(x => x.GetTrains(It.IsAny<Func<ITrain, bool>>()))
                .Returns(new List<ITrain>() { mockTrain.Object });

            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool_FPool2",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool_FPool2"),
                EnableAutoReturn = true,
                EnableFileWatcher = true,
                FileWatcherPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileWatcher_FPool1"),
            };
            IFilePool filePool = new FilePool(_mockLogger.Object, mockTrainFactory.Object, filePoolOption);
            filePool.Start();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("123456")))
            {
                await filePool.WriteFileAsync(ms, ".txt");
            }
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("hello")))
            {
                filePool.WriteFile(ms, ".txt");
            }

            Assert.Equal(2, filePool.GetPendingCount());

            mockTrain.Verify(x => x.WriteFile(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
            mockTrain.Verify(x => x.WriteFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);

            filePool.Shutdown();

            Directory.Delete(filePoolOption.Path);
            Directory.Delete(filePoolOption.FileWatcherPath);
        }

        [Fact]
        public async Task Get_Return_Release_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool_FPool3",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool_FPool3"),
                EnableAutoReturn = true,
                EnableFileWatcher = true,
                FileWatcherPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileWatcher_FPool1"),
            };
            var spoolFile = new SpoolFile()
            {
                FilePoolName = "Pool_FPool3",
                TrainIndex = 1,
                Path = Path.Combine(filePoolOption.Path, "_000001_", "0001.txt")
            };

            var mockReadTrain = new Mock<ITrain>();
            mockReadTrain.Setup(x => x.Index).Returns(1);
            mockReadTrain.Setup(x => x.GetFiles(1))
                .Returns(new SpoolFile[] { spoolFile });


            var mockWriteTrain = new Mock<ITrain>();

            var mockTrainFactory = new Mock<ITrainFactory>();
            mockTrainFactory.Setup(x => x.GetWriteTrain())
                .Returns(mockWriteTrain.Object);
            mockTrainFactory.Setup(x => x.GetReadTrain())
                .Returns(mockReadTrain.Object);
            mockTrainFactory.Setup(x => x.GetTrainByIndex(1))
                .Returns(mockReadTrain.Object);

            mockTrainFactory.Setup(x => x.GetTrains(It.IsAny<Func<ITrain, bool>>()))
                .Returns(new List<ITrain>() { mockReadTrain.Object, mockWriteTrain.Object });

            IFilePool filePool = new FilePool(_mockLogger.Object, mockTrainFactory.Object, filePoolOption);
            int onFileReturnCount = 0;
            filePool.OnFileReturn += (o, e) =>
            {
                Interlocked.Increment(ref onFileReturnCount);
            };

            filePool.Start();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("123456")))
            {
                await filePool.WriteFileAsync(ms, ".txt");
            }

            var spoolFiles = filePool.GetFiles();
            Assert.Single(spoolFiles);
            Assert.Equal(1, filePool.GetProcessingCount());

            //归还
            filePool.ReturnFiles(spoolFiles);
            Assert.Equal(0, filePool.GetProcessingCount());

            mockReadTrain.Verify(x => x.GetFiles(It.IsAny<int>()), Times.Once);
            mockReadTrain.Verify(x => x.ReturnFiles(It.IsAny<SpoolFile[]>()), Times.Once);

            //获取数据
            filePool.ReleaseFiles(spoolFiles);

            mockTrainFactory.Verify(x => x.GetTrainByIndex(It.IsAny<int>()), Times.Between(1, 3, Moq.Range.Exclusive));
            mockReadTrain.Verify(x => x.ReleaseFiles(It.IsAny<SpoolFile[]>()), Times.Once);

            Directory.Delete(filePoolOption.Path);
            Directory.Delete(filePoolOption.FileWatcherPath);
        }

    }
}
