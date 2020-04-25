using Microsoft.Extensions.Logging;
using Moq;
using Spool.Trains;
using Spool.Utility;
using Spool.Writers;
using System.IO;
using Xunit;

namespace Spool.Tests.Train
{
    public class TrainEventBindTest
    {
        private IMock<ILogger<Trains.Train>> _mockLogger;
        private IMock<FilePoolOption> _mockFilePoolOption;
        private IMock<IdGenerator> _mockIdGenerator;
        private IMock<IFileWriterManager> _mockFileWriterManager;
        private IMock<TrainOption> _mockTrainOption;

        public TrainEventBindTest()
        {
            _mockLogger = new Mock<ILogger<Trains.Train>>();
            _mockFilePoolOption = new Mock<FilePoolOption>();
            _mockIdGenerator = new Mock<IdGenerator>();
            _mockFileWriterManager = new Mock<IFileWriterManager>();
            _mockTrainOption = new Mock<TrainOption>();
        }

        [Fact]
        public void Event_Bind_UnBind_Test()
        {
            var option = new FilePoolOption()
            {
                Path = Path.GetTempPath()
            };
            var train = new Trains.Train(_mockLogger.Object, option, _mockIdGenerator.Object, _mockFileWriterManager.Object, _mockTrainOption.Object);

            train.OnDelete -= Train_OnDelete;

        }

        private void Train_OnDelete(object sender, TrainDeleteEventArgs e)
        {

        }
    }
}
