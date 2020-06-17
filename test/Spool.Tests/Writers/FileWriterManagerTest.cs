using Microsoft.Extensions.Logging;
using Moq;
using Spool.Writers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Spool.Tests.Writers
{
    public class FileWriterManagerTest
    {
        private readonly Mock<ILogger<FileWriterManager>> _mockLogger;

        public FileWriterManagerTest()
        {
            _mockLogger = new Mock<ILogger<FileWriterManager>>();
        }


        [Fact]
        public void Get_Create_Test()
        {
            var mockSpoolHost = new Mock<ISpoolHost>();

            var mockFilePoolFactory = new Mock<IFilePoolFactory>();

            var filePoolOption = new FilePoolOption()
            {
                MaxFileWriterCount = 100
            };

            IFileWriterManager fileWriterManager = new FileWriterManager(_mockLogger.Object, mockSpoolHost.Object, mockFilePoolFactory.Object, filePoolOption);


            var fileWriter = fileWriterManager.Get();
        }

    }
}
