using Microsoft.Extensions.DependencyInjection;
using Spool.Writers;
using Xunit;

namespace Spool.Tests
{
    public class FileWriterManagerTest
    {
        [Fact]
        public void Get_Return_Test()
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
                MaxFileWriterCount = 1,
                WriteBufferSize = 4096,
                EnableAutoReturn = true,
                AutoReturnSeconds = 300,
                ScanReturnFileMillSeconds = 1000

            };
            var host = provider.GetService<ISpoolHost>();
            host.SetupDI(provider);

            FileWriter fileWriter1 = null;
            using (var scope = host.Provider.CreateScope())
            {
                var fileWriter = scope.ServiceProvider.GetService<IFileWriter>();
                var option = scope.ServiceProvider.GetService<FilePoolOption>();
                option.Name = descriptor1.Name;
                option.Path = descriptor1.Path;
                option.MaxFileWriterCount = descriptor1.MaxFileWriterCount;
                option.WriteBufferSize = descriptor1.WriteBufferSize;
                option.EnableFileWatcher = descriptor1.EnableFileWatcher;
                option.FileWatcherPath = descriptor1.FileWatcherPath;
                option.EnableAutoReturn = descriptor1.EnableAutoReturn;
                option.AutoReturnSeconds = descriptor1.AutoReturnSeconds;
                option.ScanReturnFileMillSeconds = descriptor1.ScanReturnFileMillSeconds;

                fileWriter1 = scope.ServiceProvider.GetService<FileWriter>();
                Assert.NotNull(fileWriter);
            }


        }
    }
}
