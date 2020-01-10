using Microsoft.Extensions.Logging;
using System.IO;
using System.Collections.Concurrent;
using Spool.Utility;

namespace Spool.Group
{
    /// <summary>Sequence train
    /// </summary>
    public class Train
    {

        public int Index { get { return _index; } }
        public string Name { get; }
        public string TrainPath { get; }

        private readonly ConcurrentQueue<SpoolFile> _fileQueue;

        private readonly ILogger _logger;
        private readonly IdGenerator _idGenerator;
        private readonly SpoolOption _option;
        private readonly GroupPoolDescriptor _descriptor;
        private readonly int _index;


        public Train(ILogger<Train> logger, IdGenerator idGenerator, SpoolOption option, GroupPoolDescriptor descriptor, int index)
        {
            _logger = logger;
            _idGenerator = idGenerator;
            _option = option;
            _descriptor = descriptor;
            _index = index;

            Name = CreateTrainName(_index);
            TrainPath = Path.Combine(descriptor.GroupPath, Name);

            _fileQueue = new ConcurrentQueue<SpoolFile>();
        }


        public void Initialize()
        {
            if (DirectoryHelper.CreateIfNotExists(TrainPath))
            {
                _logger.LogInformation("Train create TrainPath '{0}'.", TrainPath);
            }
        }

        private string CreateTrainName(int index)
        {
            return $"_{index.ToString().PadLeft(6, '0')}_";
        }



    }
}
