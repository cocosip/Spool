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
using DotCommon.Utility;

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


            var testPath = PathUtil.MapPath("../../../test-files");
            var file2 = Path.Combine(testPath, "t1.txt");
            var spoolFile2 = await filePool.WriteFileAsync(file2);

            var spoolFiles = filePool.GetFiles(2);
            Assert.Equal(2, spoolFiles.Count);

            var spoolFile_q1 = spoolFiles.FirstOrDefault(x => x.Path == spoolFile1.Path);
            Assert.NotNull(spoolFile_q1);
            Assert.Equal("default", spoolFile_q1.FilePool);
            Assert.Equal(".txt", spoolFile_q1.FileExt);
            Assert.True(File.Exists(spoolFile_q1.Path));

            var spoolFile_q2 = spoolFiles.FirstOrDefault(x => x.Path == spoolFile2.Path);
            Assert.NotNull(spoolFile_q2);
            Assert.Equal("default", spoolFile_q2.FilePool);
            Assert.Equal(".txt", spoolFile_q2.FileExt);
            Assert.True(File.Exists(spoolFile_q2.Path));

            Assert.True(Directory.Exists(configuration.Path));
            Directory.Delete(configuration.Path, true);

        }

    }
}
