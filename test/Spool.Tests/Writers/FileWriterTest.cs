using Microsoft.Extensions.Logging;
using Moq;
using Spool.Writers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Spool.Tests.Writers
{
    public class FileWriterTest
    {
        private readonly Mock<ILogger<FileWriter>> _mockLogger;
        public FileWriterTest()
        {
            _mockLogger = new Mock<ILogger<FileWriter>>();
        }



        [Fact]
        public void WriteFile_Test()
        {
            IFileWriter fileWriter = new FileWriter(_mockLogger.Object, new FilePoolOption()
            {
                WriteBufferSize = 1024 * 1024
            });

            var path = Path.Combine(AppContext.BaseDirectory, "WriteFile.txt");

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello world!")))
            {
                fileWriter.WriteFile(ms, path);
            }

            Assert.True(File.Exists(path));
            File.Delete(path);
        }

        [Fact]
        public async Task WriteFileAsync_Test()
        {
            IFileWriter fileWriter = new FileWriter(_mockLogger.Object, new FilePoolOption()
            {
                WriteBufferSize = 1024 * 1024
            });

            var path = Path.Combine(AppContext.BaseDirectory, "WriteFileAsync.txt");

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello world!")))
            {
                await fileWriter.WriteFileAsync(ms, path);
            }

            Assert.True(File.Exists(path));
            File.Delete(path);
        }

    }
}
