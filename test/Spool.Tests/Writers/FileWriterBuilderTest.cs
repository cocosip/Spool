using Microsoft.Extensions.DependencyInjection;
using Moq;
using Spool.Writers;
using System;
using Xunit;

namespace Spool.Tests.Writers
{
    public class FileWriterBuilderTest
    {
        [Fact]
        public void BuildWriter_Test()
        {

            var mockFileWriter = new Mock<IFileWriter>();

            var fileWriterOption = new FileWriterOption();


            var mockScopeServiceProvider = new Mock<IServiceProvider>();
            mockScopeServiceProvider.Setup(x => x.GetService(typeof(IFileWriter)))
                .Returns(mockFileWriter.Object);

            mockScopeServiceProvider.Setup(x => x.GetService(typeof(FileWriterOption)))
                 .Returns(fileWriterOption);

            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(x => x.ServiceProvider)
                .Returns(mockScopeServiceProvider.Object);

            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceScopeFactory.Setup(x => x.CreateScope())
                .Returns(mockScope.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockServiceScopeFactory.Object);


            IFileWriterBuilder fileWriterBuilder = new FileWriterBuilder(mockServiceProvider.Object);

            var filePoolOption = new FilePoolOption()
            {
                Name = "Pool1",
                Path = "D:\\Pool1",
                MaxFileWriterCount = 20,
                ConcurrentFileWriterCount = 4,
                WriteBufferSize = 1024
            };

            var fileWriter = fileWriterBuilder.BuildWriter(filePoolOption);
            Assert.NotNull(fileWriter);
            Assert.Equal("Pool1", fileWriterOption.Name);
            Assert.Equal("D:\\Pool1", fileWriterOption.Path);
            Assert.Equal(20, fileWriterOption.MaxFileWriterCount);
            Assert.Equal(4, fileWriterOption.ConcurrentFileWriterCount);
            Assert.Equal(1024, fileWriterOption.WriteBufferSize);


            mockScopeServiceProvider.Verify(x => x.GetService(typeof(FileWriterOption)), Times.Once);
            mockScopeServiceProvider.Verify(x => x.GetService(typeof(IFileWriter)), Times.Once);

        }

    }
}
