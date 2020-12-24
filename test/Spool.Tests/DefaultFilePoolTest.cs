using DotCommon.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spool.Utility;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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

            var pending1 = filePool.GetPendingCount();
            Assert.Equal(2, pending1);
            var processing1 = filePool.GetProcessingCount();
            Assert.Equal(0, processing1);

            //Write third
            var spoolFile3 = filePool.WriteFile(file2);

            var spoolFiles = filePool.GetFiles(2);
            Assert.Equal(2, spoolFiles.Count);

            var pending2 = filePool.GetPendingCount();
            Assert.Equal(1, pending2);
            var processing2 = filePool.GetProcessingCount();
            Assert.Equal(2, processing2);

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
