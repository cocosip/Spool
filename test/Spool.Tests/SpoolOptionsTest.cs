using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using Xunit;

namespace Spool.Tests
{
    public class SpoolOptionsTest
    {
        [Fact]
        public void SpoolOptions_Configurations_Test()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.Configure<SpoolOptions>(options =>
            {
                options.FilePools.Configure("test-spool1", c =>
                {
                    c.Name = "test-spool1";
                    c.Path = Path.Combine(AppContext.BaseDirectory, "test-spool1");
                    c.WriteBufferSize = 1024 * 1024 * 2;
                    c.TrainMaxFileCount = 300;
                    c.EnableFileWatcher = true;
                    c.FileWatcherPath = Path.Combine(AppContext.BaseDirectory, "test-spool1-watcher");
                    c.FileWatcherCopyThread = 2;
                    c.ScanFileWatcherMillSeconds = 3000;

                    c.EnableAutoReturn = true;
                    c.AutoReturnSeconds = 200;
                    c.ScanReturnFileMillSeconds = 2000;
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetService<IOptions<SpoolOptions>>().Value;
            var configuration = options.FilePools.GetConfiguration("test-spool1");

            Assert.Equal("test-spool1", configuration.Name);
            Assert.EndsWith("test-spool1", configuration.Path);
            Assert.Equal(1024 * 1024 * 2, configuration.WriteBufferSize);
            Assert.Equal(300, configuration.TrainMaxFileCount);
            Assert.True(configuration.EnableFileWatcher);
            Assert.EndsWith("test-spool1-watcher", configuration.FileWatcherPath);
            Assert.Equal(2, configuration.FileWatcherCopyThread);
            Assert.Equal(3000, configuration.ScanFileWatcherMillSeconds);
            Assert.True(configuration.EnableAutoReturn);
            Assert.Equal(200, configuration.AutoReturnSeconds);
            Assert.Equal(2000, configuration.ScanReturnFileMillSeconds);
        }

    }
}
