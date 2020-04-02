using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Linq;

namespace Spool.Demo
{
    class Program
    {
        static IServiceProvider Provider { get; set; }
        static void Main(string[] args)
        {

            var spoolOption = new SpoolOption()
            {
                DefaultPool = "pool1"
            };
            for (int i = 0; i < 5; i++)
            {
                spoolOption.FilePools.Add(new FilePoolDescriptor()
                {
                    Name = $"pool{i}",
                    Path = $"D:\\pool{i}",
                   // MaxFileWriterCount = 50,
                    TrainMaxFileCount = 65535,
                    WriteBufferSize = 1024 * 1024 * 2,
                    EnableAutoReturn = true,
                    AutoReturnSeconds = 10,
                    ScanReturnFileMillSeconds = 1000,
                    EnableFileWatcher = true,
                    FileWatcherPath = $"D:\\pool_watcher{i}",
                    ScanFileWatcherMillSeconds = 5000,
                });
            }


            IServiceCollection services = new ServiceCollection();

            services
                .AddLogging(l =>
                {
                    l.AddConsole()
                    .SetMinimumLevel(LogLevel.Debug);
                })
                .AddSpool(spoolOption);

            Provider = services.BuildServiceProvider();
            Provider.UseSpool();

            Console.WriteLine("初始化完成!");

            Run();

            Console.ReadLine();
        }

        public static void Run()
        {
            WriteFileTest();
            GetFileTest();

        }

        public static void WriteFileTest()
        {
            var spoolPool = Provider.GetService<SpoolPool>();

            for (int i = 0; i < 5; i++)
            {
                var total = 500000;
                var currentWrite = 0;
                var buffer = File.ReadAllBytes(@"D:\DicomTests\FILE0.dcm");
                var poolName = $"pool{i}";
                var ext = ".dcm";
                Task.Run(() =>
                {
                    while (currentWrite < total)
                    {
                        //写入
                        spoolPool.WriteAsync(poolName, buffer, ext).ContinueWith(t =>
                        {
                            if (t.IsCompleted)
                            {
                                currentWrite++;
                            }
                        });
                    }

                });

                Task.Run(() =>
                {
                    Console.WriteLine("文件池:{0} 已写入文件数:{1}", poolName, currentWrite);
                    Thread.Sleep(1000);
                });
            }



        }


        public static void GetFileTest()
        {
            var spoolPool = Provider.GetService<SpoolPool>();
            for (var i = 0; i < 5; i++)
            {
                var poolName = $"pool{i}";
                Task.Run(() =>
                {
                    while (true)
                    {
                        var spoolFiles = new SpoolFile[0] { };
                        try
                        {
                            spoolFiles = spoolPool.Get(poolName, 50);
                            //释放
                            spoolPool.Release(poolName, spoolFiles);
                            spoolFiles = new SpoolFile[0] { };

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("文件池:'{0}'获取文件出错,异常信息:{1}.", poolName, ex.Message);
                        }
                        finally
                        {
                            if (spoolFiles != null && spoolFiles.Length > 0)
                            {
                                spoolPool.Return(poolName, spoolFiles);
                            }
                        }

                        Thread.Sleep(2000);
                    }
                });
            }

        }


    }
}
