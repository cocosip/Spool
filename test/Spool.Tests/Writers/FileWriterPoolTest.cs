using Microsoft.Extensions.Logging;
using Moq;
using Spool.Writers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Spool.Tests.Writers
{
    public class FileWriterPoolTest
    {
        private readonly Mock<ILogger<FileWriterPool>> _mockLogger;

        public FileWriterPoolTest()
        {
            _mockLogger = new Mock<ILogger<FileWriterPool>>();
        }

        [Fact]
        public void Get_BuildWriter_Test()
        {
            FilePoolOption filePoolOption = new FilePoolOption()
            {
                MaxFileWriterCount = 10,
            };

            var mockFileWriterLogger = new Mock<ILogger<FileWriter>>();
            var mockFileWriterBuilder = new Mock<IFileWriterBuilder>();
            mockFileWriterBuilder.Setup(x => x.BuildWriter(It.IsAny<FilePoolOption>()))
                .Returns(new FileWriter(mockFileWriterLogger.Object, filePoolOption));

            IFileWriterPool fileWriterPool = new FileWriterPool(_mockLogger.Object, filePoolOption, mockFileWriterBuilder.Object);

            var fileWriter = fileWriterPool.Get();

            mockFileWriterBuilder.Verify(x => x.BuildWriter(It.IsAny<FilePoolOption>()), Times.Once);
        }



        [Fact]
        public void GetAndReturn_Test()
        {
            FilePoolOption filePoolOption = new FilePoolOption()
            {
                MaxFileWriterCount = 1,
            };

            var mockFileWriterLogger = new Mock<ILogger<FileWriter>>();

            var mockFileWriter = new Mock<IFileWriter>();
            mockFileWriter.Setup(x => x.Id)
                .Returns("123456");

            var mockFileWriterBuilder = new Mock<IFileWriterBuilder>();
            mockFileWriterBuilder.Setup(x => x.BuildWriter(It.IsAny<FilePoolOption>()))
                .Returns(mockFileWriter.Object);

            IFileWriterPool fileWriterPool = new FileWriterPool(_mockLogger.Object, filePoolOption, mockFileWriterBuilder.Object);

            var fileWriter1 = fileWriterPool.Get();

            fileWriterPool.Return(fileWriter1);

            var fileWriter2 = fileWriterPool.Get();

            Assert.Equal(fileWriter1.Id, fileWriter2.Id);
            Assert.Equal("123456", fileWriter1.Id);

            mockFileWriterBuilder.Verify(x => x.BuildWriter(It.IsAny<FilePoolOption>()), Times.Once);


        }

    }
}
