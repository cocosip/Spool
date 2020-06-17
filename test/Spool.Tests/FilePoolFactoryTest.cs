using Microsoft.Extensions.DependencyInjection;
using Moq;
using Spool.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Spool.Tests
{
    public class FilePoolFactoryTest
    {
        private readonly Mock<ISpoolHost> _mockSpoolHost;

        public FilePoolFactoryTest()
        {
            _mockSpoolHost = new Mock<ISpoolHost>();
        }

        [Fact]
        public void BuildOption_Test()
        {
            IFilePoolFactory filePoolFactory = new FilePoolFactory(_mockSpoolHost.Object);

            var descriptor = new FilePoolDescriptor()
            {
                Name = "Pool1",
                Path = "D:\\Pool1",
                TrainMaxFileCount = 100,
                WriteBufferSize = 1024,
                MaxFileWriterCount = 10,
                EnableAutoReturn = false,
                AutoReturnSeconds = 10,
                EnableFileWatcher = true,
                FileWatcherPath = "D:\\FileWatcher1",
                ScanFileWatcherMillSeconds = 1000,
                ScanReturnFileMillSeconds = 5
            };

            var option = filePoolFactory.BuildOption(descriptor);
            Assert.Equal(descriptor.Name, option.Name);
            Assert.Equal(descriptor.Path, option.Path);
            Assert.Equal(descriptor.TrainMaxFileCount, option.TrainMaxFileCount);
            Assert.Equal(descriptor.WriteBufferSize, option.WriteBufferSize);
            Assert.Equal(descriptor.MaxFileWriterCount, option.MaxFileWriterCount);
            Assert.Equal(descriptor.EnableAutoReturn, option.EnableAutoReturn);
            Assert.Equal(descriptor.AutoReturnSeconds, option.AutoReturnSeconds);
            Assert.Equal(descriptor.EnableFileWatcher, option.EnableFileWatcher);
            Assert.Equal(descriptor.FileWatcherPath, option.FileWatcherPath);
            Assert.Equal(descriptor.ScanFileWatcherMillSeconds, option.ScanFileWatcherMillSeconds);
            Assert.Equal(descriptor.ScanReturnFileMillSeconds, option.ScanReturnFileMillSeconds);

        }

        [Fact]
        public void CreateFilePool_Test()
        {
            IServiceCollection services = new ServiceCollection();
            var descriptor = new FilePoolDescriptor()
            {
                Name = "Pool1",
                Path = "D:\\Pool1",
                TrainMaxFileCount = 100,
                WriteBufferSize = 1024,
                MaxFileWriterCount = 10,
                EnableAutoReturn = false,
                AutoReturnSeconds = 10,
                EnableFileWatcher = true,
                FileWatcherPath = "D:\\FileWatcher1",
                ScanFileWatcherMillSeconds = 1000,
                ScanReturnFileMillSeconds = 5
            };
            services
                .AddLogging()
                .AddSpool(c =>
                {
                    c.FilePools.Add(descriptor);
                });

            var provider = services.BuildServiceProvider()
                .ConfigureSpool();

            var factory = provider.GetService<IFilePoolFactory>();
            var filePool = factory.CreateFilePool(descriptor);
            var option = filePool.Option;
            Assert.Equal(descriptor.Name, option.Name);
            Assert.Equal(descriptor.Path, option.Path);
            Assert.Equal(descriptor.TrainMaxFileCount, option.TrainMaxFileCount);
            Assert.Equal(descriptor.WriteBufferSize, option.WriteBufferSize);
            Assert.Equal(descriptor.MaxFileWriterCount, option.MaxFileWriterCount);
            Assert.Equal(descriptor.EnableAutoReturn, option.EnableAutoReturn);
            Assert.Equal(descriptor.AutoReturnSeconds, option.AutoReturnSeconds);
            Assert.Equal(descriptor.EnableFileWatcher, option.EnableFileWatcher);
            Assert.Equal(descriptor.FileWatcherPath, option.FileWatcherPath);
            Assert.Equal(descriptor.ScanFileWatcherMillSeconds, option.ScanFileWatcherMillSeconds);
            Assert.Equal(descriptor.ScanReturnFileMillSeconds, option.ScanReturnFileMillSeconds);

            //DirectoryHelper.DeleteIfExist(descriptor.Path, true);
            //DirectoryHelper.DeleteIfExist(descriptor.FileWatcherPath, true);
        }

    }
}
