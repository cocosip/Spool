using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using Spool.Trains;
using Spool.Utility;
using Spool.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace Spool.Tests.Trains
{
    public class TrainTest
    {
        private readonly Mock<ILogger<Train>> _mockLogger;
        public TrainTest()
        {
            _mockLogger = new Mock<ILogger<Train>>();
        }

        [Fact]
        public void Initialize_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool10",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool10")
            };
            var trainOption = new TrainOption()
            {
                Index = 1
            };
            var mockIdGenerator = new Mock<IdGenerator>();
            var mockFileWriterPool = new Mock<IFileWriterPool>();

            ITrain train = new Train(_mockLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, trainOption);
            train.Initialize();

            Assert.Equal(1, train.Index);
            Assert.Equal(TrainType.Default, train.TrainType);
            Assert.True(Directory.Exists(train.Path));
            Assert.True(train.IsEmpty());

            Directory.Delete(train.Path, true);
            Assert.False(Directory.Exists(train.Path));
            train.Initialize();
            Assert.False(Directory.Exists(train.Path));

        }

        [Fact]
        public async Task WriteFileAsync_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool11",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool11")
            };
            var trainOption = new TrainOption()
            {
                Index = 1
            };

            var mockIdGenerator = new Mock<IdGenerator>();

            var mockFileWriter = new Mock<IFileWriter>();
            mockFileWriter.Setup(x => x.Id).Returns("123456");
            var mockFileWriterPool = new Mock<IFileWriterPool>();
            mockFileWriterPool.Setup(x => x.Get()).Returns(mockFileWriter.Object);

            ITrain train = new Train(_mockLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, trainOption);
            train.Initialize();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("hello world!")))
            {
                await train.WriteFileAsync(ms, ".txt");
            }

            Assert.False(train.IsEmpty());
            mockFileWriter.Verify(x => x.WriteFile(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
            mockFileWriterPool.Verify(x => x.Return(It.Is<IFileWriter>(x => x.Id == "123456")), Times.Once);

            Directory.Delete(train.Path, true);
        }

        [Fact]
        public async Task WriteFileAsync_WriteOver_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool12",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool12"),
                TrainMaxFileCount = 1
            };
            var trainOption = new TrainOption()
            {
                Index = 1
            };

            var mockIdGenerator = new Mock<IdGenerator>();

            var mockFileWriter = new Mock<IFileWriter>();
            mockFileWriter.Setup(x => x.Id).Returns("123456");
            var mockFileWriterPool = new Mock<IFileWriterPool>();
            mockFileWriterPool.Setup(x => x.Get()).Returns(mockFileWriter.Object);

            ITrain train = new Train(_mockLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, trainOption);
            var writeOverCount = 0;
            train.OnWriteOver += (o, e) =>
            {
                Interlocked.Increment(ref writeOverCount);

                Assert.Equal("Pool12", e.Info.FilePoolName);
                Assert.Equal(filePoolOption.Path, e.Info.FilePoolPath);
            };

            train.Initialize();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("hello world!")))
            {
                await train.WriteFileAsync(ms, ".txt");
            }

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("abc123")))
            {
                await train.WriteFileAsync(ms, ".txt");
            }

            Assert.Equal(2, train.PendingCount);
            Assert.Equal(1, writeOverCount);

            mockFileWriter.Verify(x => x.WriteFile(It.IsAny<Stream>(), It.IsAny<string>()), Times.Between(1, 3, Moq.Range.Exclusive));
            mockFileWriterPool.Verify(x => x.Return(It.Is<IFileWriter>(x => x.Id == "123456")), Times.Between(1, 3, Moq.Range.Exclusive));

            Directory.Delete(train.Path, true);
        }

        [Fact]
        public void WriteFile_Exception_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool13",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool13"),
                TrainMaxFileCount = 10
            };
            var trainOption = new TrainOption()
            {
                Index = 1
            };

            var mockIdGenerator = new Mock<IdGenerator>();

            var mockFileWriter = new Mock<IFileWriter>();
            mockFileWriter.Setup(x => x.Id).Returns("123456");
            mockFileWriter.Setup(x => x.WriteFile(It.IsAny<Stream>(), It.IsAny<string>())).Throws<IOException>();

            var mockFileWriterPool = new Mock<IFileWriterPool>();
            mockFileWriterPool.Setup(x => x.Get()).Returns(mockFileWriter.Object);

            ITrain train = new Train(_mockLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, trainOption);
            var writeOverCount = 0;
            train.OnWriteOver += (o, e) =>
            {
                Interlocked.Increment(ref writeOverCount);
            };

            train.Initialize();

            Assert.Throws<IOException>(() =>
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("hello world!")))
                {
                    train.WriteFile(ms, ".txt");
                }
            });

            Assert.Equal(0, train.PendingCount);
            Assert.Equal(0, writeOverCount);


            Directory.Delete(train.Path, true);
        }

        [Fact]
        public async Task GetFiles_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool14",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool14"),
                TrainMaxFileCount = 10
            };
            var trainOption = new TrainOption()
            {
                Index = 1
            };

            var mockIdGenerator = new Mock<IdGenerator>();

            var mockFileWriter = new Mock<IFileWriter>();
            mockFileWriter.Setup(x => x.Id).Returns("123456");
            //mockFileWriter.Setup(x => x.WriteFile(It.IsAny<Stream>(), It.IsAny<string>()));


            var mockFileWriterPool = new Mock<IFileWriterPool>();
            mockFileWriterPool.Setup(x => x.Get()).Returns(mockFileWriter.Object);

            ITrain train = new Train(_mockLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, trainOption);
            train.Initialize();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("hello world!")))
            {
                await train.WriteFileAsync(ms, ".txt");
            }

            var spoolFiles1 = train.GetFiles(2);
            Assert.Single(spoolFiles1);
            Assert.Equal(0, train.PendingCount);
            Assert.Equal(1, train.ProgressingCount);

        }

        [Fact]
        public async Task ReleaseFiles_Test()
        {
            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool15",
                Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pool15"),
                TrainMaxFileCount = 10
            };
            var trainOption = new TrainOption()
            {
                Index = 1
            };

            var mockIdGenerator = new Mock<IdGenerator>();

            var mockFileWriter = new Mock<IFileWriter>();
            mockFileWriter.Setup(x => x.Id).Returns("123456");
            //mockFileWriter.Setup(x => x.WriteFile(It.IsAny<Stream>(), It.IsAny<string>()));

            var mockFileWriterPool = new Mock<IFileWriterPool>();
            mockFileWriterPool.Setup(x => x.Get()).Returns(mockFileWriter.Object);

            ITrain train = new Train(_mockLogger.Object, filePoolOption, mockIdGenerator.Object, mockFileWriterPool.Object, trainOption);
            train.Initialize();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("hello world!")))
            {
                await train.WriteFileAsync(ms, ".txt");
            }
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("1234567890")))
            {
                await train.WriteFileAsync(ms, ".txt");
            }
            var spoolFiles1 = train.GetFiles(3);

            Assert.Equal(2, spoolFiles1.Length);
            Assert.Equal(0, train.PendingCount);
            Assert.Equal(2, train.ProgressingCount);
            train.ReleaseFiles(spoolFiles1);
            Assert.Equal(0, train.PendingCount);
            Assert.Equal(0, train.ProgressingCount);

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("xxxx")))
            {
                await train.WriteFileAsync(ms, ".txt");
            }

            var spoolFiles2 = train.GetFiles();
            Assert.Single(spoolFiles2);
            Assert.Equal(0, train.PendingCount);
            Assert.Equal(1, train.ProgressingCount);
            var releaseSpoolFile = new SpoolFile()
            {
                FilePoolName = spoolFiles2[0].FilePoolName,
                TrainIndex = spoolFiles2[0].TrainIndex,
                Path = spoolFiles2[0].Path + "_rm"
            };
            train.ReleaseFiles(releaseSpoolFile);
            Assert.Equal(0, train.PendingCount);
            Assert.Equal(1, train.ProgressingCount);
            train.ReleaseFiles(spoolFiles2);
            Assert.Equal(0, train.PendingCount);
            Assert.Equal(0, train.ProgressingCount);

        }
 
    }
}
