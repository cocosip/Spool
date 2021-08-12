using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spool.Sample
{
    class Program
    {
        static IFilePoolFactory _filePoolFactory;
        static ILogger _logger;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            IServiceCollection services = new ServiceCollection();
            services
                .AddLogging(l =>
                {
                    l.AddConsole();
                })
                .AddSpool();

            services.Configure<SpoolOptions>(options =>
            {
                options.FilePools.ConfigureDefault(c =>
                {
                    c.Name = DefaultFilePool.Name;
                    c.Path = "E:\\SpoolTest";
                    c.EnableFileWatcher = true;
                    c.FileWatcherPath = "D:\\SpoolWatcher";
                    c.EnableAutoReturn = true;
                    c.FileWatcherLastWrite = 3;
                    c.FileWatcherSkipZeroFile = true;
                    c.FileWatcherCopyThread = 2;
                    c.ScanReturnFileMillSeconds = 2000;
                    c.AutoReturnSeconds = 300;
                });
            });

            var serviceProvider = services.BuildServiceProvider();
            _filePoolFactory = serviceProvider.GetService<IFilePoolFactory>();
            _logger = serviceProvider.GetService<ILogger<Program>>();

            var filePool = _filePoolFactory.GetOrCreate(DefaultFilePool.Name);

            //Run();

            Console.ReadLine();
        }

        public static void Run()
        {
            Task.Run(async () =>
            {
                var count = 0;
                var data = Encoding.UTF8.GetBytes("Hello,it's spool!");
                while (count <= 100000)
                {
                    try
                    {
                        var filePool = _filePoolFactory.GetOrCreate<DefaultFilePool>();
                        await filePool.WriteFileAsync(new MemoryStream(data), ".txt");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "写入出错");
                    }
                    Interlocked.Increment(ref count);
                }
            });
        }

    }
}
