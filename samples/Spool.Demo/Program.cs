﻿using Microsoft.Extensions.DependencyInjection;
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

        static int FilePoolCount = 1;
        static void Main(string[] args)
        {

            var spoolOption = new SpoolOption()
            {
                DefaultPool = "pool1"
            };
            for (int i = 0; i < FilePoolCount; i++)
            {
                spoolOption.FilePools.Add(new FilePoolDescriptor()
                {
                    Name = $"pool{i}",
                    Path = $"D:\\SpoolTest\\pool{i}",
                    MaxFileWriterCount = 50,
                    TrainMaxFileCount = 1000,
                    WriteBufferSize = 1024 * 1024 * 2,
                    EnableAutoReturn = true,
                    AutoReturnSeconds = 10,
                    ScanReturnFileMillSeconds = 1000,
                    EnableFileWatcher = false,
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
            Provider.ConfigureSpool();

            Console.WriteLine("初始化完成!");

            Run();

            Console.ReadLine();
        }

        public static void Run()
        {
            WriteFileTest();
            //GetFileTest();
        }

        public static void WriteFileTest()
        {
            var spoolPool = Provider.GetService<ISpoolPool>();
            var buffer = File.ReadAllBytes(@"D:\FILE0.dcm");
            for (int i = 0; i < FilePoolCount; i++)
            {
                var total = 100000;
                var currentWrite = 0;
                //var buffer = File.ReadAllBytes(@"D:\FILE0.dcm");
                var poolName = $"pool{i}";
                var ext = ".dcm";
                Task.Run(async () =>
                {
                    while (currentWrite < total)
                    {
                        //写入
                        await spoolPool.WriteAsync(buffer, ext, poolName);
                        Interlocked.Increment(ref currentWrite);
                        Console.WriteLine("当前文件池数量:{0}", spoolPool.GetPendingCount(poolName));
                        //Thread.Sleep(50);
                    }

                });

                Task.Run(() =>
                {
                    while (true)
                    {
                        Console.WriteLine("文件池:{0} 已写入文件数:{1}", poolName, currentWrite);
                        Thread.Sleep(1000);
                    }
                });
            }
        }
    

        public static void GetFileTest()
        {
            var spoolPool = Provider.GetService<ISpoolPool>();
            for (var i = 0; i < FilePoolCount; i++)
            {
                var poolName = $"pool{i}";
                Task.Run(() =>
                {
                    while (true)
                    {
                        var spoolFiles = new SpoolFile[0] { };
                        try
                        {
                            spoolFiles = spoolPool.Get(50, poolName);
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
