using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Spool.Demo
{
    class Program
    {
        static IServiceProvider Provider { get; set; }
        static void Main(string[] args)
        {

            var descriptor1 = new FilePoolDescriptor()
            {
                Name = "group1",
                Path = "D:\\Group1",
                MaxFileWriterCount = 10,
                TrainMaxFileCount = 200,
                WriteBufferSize = 1024 * 1024 * 2,
                EnableAutoReturn = false,
                EnableFileWatcher = false
            };

            var descriptor2 = new FilePoolDescriptor()
            {
                Name = "group2",
                Path = "D:\\Group2",
                MaxFileWriterCount = 10,
                TrainMaxFileCount = 200,
                WriteBufferSize = 1024 * 1024 * 2,
                EnableAutoReturn = false,
                EnableFileWatcher = false
            };

            IServiceCollection services = new ServiceCollection();
            services.AddLogging(l =>
            {
                l.AddConsole();
            }).AddSpool(o =>
            {
                o.DefaultPool = "group1";
                o.FilePools = new List<FilePoolDescriptor>()
                {
                     descriptor1,
                     //descriptor2
                };
            });

            Provider = services.BuildServiceProvider();
            Provider.UseSpool();

            Console.WriteLine("初始化完成!");

            Run();

            Console.ReadLine();
        }

        public static void Run()
        {
            WriteFileTest();
            //WriteFileTest2();
            GetFileTest();

        }

        public static void WriteFileTest()
        {
            var spoolPool = Provider.GetService<SpoolPool>();
            int totalCount = 1000;
            Task.Run(async () =>
            {
                var index = 0;
                var file = File.ReadAllBytes(@"D:\DicomTests\FILE0.dcm");
                while (index < totalCount)
                {
                    var spoolFile = await spoolPool.WriteAsync(file, ".dcm");
                    Console.WriteLine("写入文件:文件池:{0},序列:{1},路径:{2}", spoolFile.FilePoolName, spoolFile.TrainIndex, spoolFile.Path);
                    index++;
                }

            });
        }


        public static void GetFileTest()
        {
            var spoolPool = Provider.GetService<SpoolPool>();
            Task.Run(() =>
            {
                var spoolFiles = new List<SpoolFile>();
                while (true)
                {
                    try
                    {
                        spoolFiles = spoolPool.Get(100, "group1");
                        if (spoolFiles.Count < 100)
                        {
                            var a = 1;
                        }

                        Console.WriteLine("本次获取文件数量:{0}", spoolFiles.Count);
                    }
                    catch (Exception ex)
                    {
                        if (spoolFiles != null && spoolFiles.Count > 0)
                        {
                            spoolPool.Return(spoolFiles);
                        }
                        throw ex;
                    }
                    finally
                    {
                        if (spoolFiles != null && spoolFiles.Count > 0)
                        {
                            spoolPool.Release(spoolFiles);
                        }
                    }
                    Thread.Sleep(500);
                }

            });
        }


    }
}
