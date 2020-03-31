using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Spool.Tests
{
    public class FilePoolManagerTest
    {
        [Fact]
        public void CreateFilePool_Test()
        {
            IServiceCollection services = new ServiceCollection();
            services
                .AddLogging()
                .AddSpool();
            var provider = services.BuildServiceProvider();

            var descriptor1 = new FilePoolDescriptor()
            {
                Name = "Pool1",
                Path = "D:\\Test1",
                EnableFileWatcher = false,
                FileWatcherPath = "",
                MaxFileWriterCount = 100,
                WriteBufferSize = 4096
            };
            var host = provider.GetService<ISpoolHost>();
            host.SetupDI(provider);

            var filePoolManager = provider.GetService<IFilePoolFactory>();
            var filePool1 = filePoolManager.CreateFilePool(descriptor1);
            var option1 = filePool1.Option;

            Assert.Equal(descriptor1.Name, option1.Name);
            Assert.Equal(descriptor1.Path, option1.Path);
            Assert.Equal(descriptor1.EnableFileWatcher, option1.EnableFileWatcher);
            Assert.Equal(descriptor1.FileWatcherPath, option1.FileWatcherPath);
            Assert.Equal(descriptor1.MaxFileWriterCount, option1.MaxFileWriterCount);
            Assert.Equal(descriptor1.WriteBufferSize, option1.WriteBufferSize);
        }

    }
}
