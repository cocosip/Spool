using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Options;
using System.Linq;
using Spool.Utility;

namespace Spool.Tests
{
    public class DefaultFilePoolTest : SpoolTestBase
    {
        private readonly SpoolOptions _options;
        private readonly IFilePoolFactory _filePoolFactory;
        public DefaultFilePoolTest()
        {
            _options = ServiceProvider.GetService<IOptions<SpoolOptions>>().Value;
            _filePoolFactory = ServiceProvider.GetService<IFilePoolFactory>();
        }

        [Fact]
        public async Task WriteFile_GetFile_Test()
        {
            var configuration = _options.FilePools.GetConfiguration<DefaultFilePool>();
            FilePathUtil.DeleteDirIfExist(configuration.Path, true);

             var filePool = _filePoolFactory.GetOrCreate<DefaultFilePool>();
            var content = Encoding.UTF8.GetBytes("Hello,Spool Unit Test!");
            var spoolFile1 = await filePool.WriteFileAsync(new MemoryStream(content), ".txt");
            Assert.Equal(DefaultFilePool.Name, spoolFile1.FilePool);
            Assert.Equal(1, spoolFile1.TrainIndex);

            var spoolFiles = filePool.GetFiles(2);
            Assert.Single(spoolFiles);

            var spoolFile2 = spoolFiles.FirstOrDefault();
            Assert.Equal(spoolFile1.Path, spoolFile2.Path);

            Assert.True(Directory.Exists(configuration.Path));
            Directory.Delete(configuration.Path, true);

        }

    }
}
