using Microsoft.Extensions.DependencyInjection;
using Spool.Utility;
using System;
using System.IO;

namespace Spool.Tests
{
    public class SpoolTestBase
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        public SpoolTestBase()
        {
            FilePathUtil.DeleteDirIfExist(Path.Combine(AppContext.BaseDirectory, "default-pool"), true);

            IServiceCollection services = new ServiceCollection();
            services.AddLogging()
                .Configure<SpoolOptions>(options =>
                {
                    options.FilePools.ConfigureDefault(c =>
                    {
                        c.Name = DefaultFilePool.Name;
                        c.Path = Path.Combine(AppContext.BaseDirectory, "default-pool");
                        c.WriteBufferSize = 1024 * 1024 * 5;
                        c.TrainMaxFileCount = 2;
                        c.EnableFileWatcher = true;
                        c.FileWatcherPath = Path.Combine(AppContext.BaseDirectory, "default-pool-watcher");
                        c.FileWatcherCopyThread = 2;
                        c.ScanFileWatcherMillSeconds = 500;

                        c.EnableAutoReturn = true;
                        c.AutoReturnSeconds = 1;
                        c.ScanReturnFileMillSeconds = 500;
                    });
                })
                .AddSpool();

            ServiceProvider = services.BuildServiceProvider();
        }


    }
}
