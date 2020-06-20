using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spool.Scheduling;
using Spool.Trains;
using Spool.Utility;
using Spool.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Spool.Tests
{
    public class ServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddSpool_Test()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging()
                .AddSpool(new SpoolOption()
                {
                    DefaultPool = "Pool111",
                    FilePools = new List<FilePoolDescriptor>()
                    {
                        new FilePoolDescriptor()
                        {
                            Name="Pool2",
                            Path= Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Setup_Pool2")
                        }
                    }
                });

            var provider = services.BuildServiceProvider();
            provider.ConfigureSpool();

            Assert.Contains(services, x => x.ServiceType == typeof(IScheduleService));
            Assert.Contains(services, x => x.ServiceType == typeof(ISpoolPool));
            Assert.Contains(services, x => x.ServiceType == typeof(IdGenerator));
            Assert.Contains(services, x => x.ServiceType == typeof(IFilePoolFactory));
            Assert.Contains(services, x => x.ServiceType == typeof(IFilePool));
            Assert.Contains(services, x => x.ServiceType == typeof(FilePoolOption));
            Assert.Contains(services, x => x.ServiceType == typeof(IFileWriterBuilder));
            Assert.Contains(services, x => x.ServiceType == typeof(IFileWriterPool));
            Assert.Contains(services, x => x.ServiceType == typeof(IFileWriter));
            Assert.Contains(services, x => x.ServiceType == typeof(FileWriterOption));
            Assert.Contains(services, x => x.ServiceType == typeof(ITrainBuilder));
            Assert.Contains(services, x => x.ServiceType == typeof(ITrainFactory));
            Assert.Contains(services, x => x.ServiceType == typeof(ITrain));
            Assert.Contains(services, x => x.ServiceType == typeof(TrainOption));
            var option = provider.GetService<IOptions<SpoolOption>>().Value;

            Assert.Equal("Pool2", option.DefaultPool);
            Assert.Single(option.FilePools);

        }

    }
}
