using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spool.Demo
{
    class Program
    {
        static IServiceProvider Provider { get; set; }
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging(l =>
            {
                l.AddConsole();
            }).AddSpool(o =>
            {
                o.DefaultPool = "group1";
                o.FilePools = new List<FilePoolDescriptor>()
                {
                    new FilePoolDescriptor()
                    {
                        Name="group1",
                        Path="D:\\Group1",
                        MaxFileWriterCount=10,
                        TrainMaxFileCount=65535,
                        WriteBufferSize=1024*1024*2 ,
                        EnableAutoReturn=false,
                        EnableFileWatcher=false
                    },
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
            var spoolPool = Provider.GetService<SpoolPool>();
            int totalCount = 1000;
            Task.Run(async () =>
            {
                var index = 0;
                while (index < totalCount)
                {
                    var spoolFile = await spoolPool.WriteAsync(@"D:\DicomTests\FILE0.dcm");
                    Console.WriteLine("写入文件:文件池:{0},序列:{1},路径:{2}", spoolFile.FilePoolName, spoolFile.TrainIndex, spoolFile.Path);
                }

            });

            //Task.Run(() =>
            //{

            //});


        }
    }
}
