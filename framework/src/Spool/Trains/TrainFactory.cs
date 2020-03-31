using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spool.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spool.Trains
{
    /// <summary>序列工厂
    /// </summary>
    public class TrainFactory : ITrainFactory
    {
        private readonly ILogger _logger;
        private readonly ISpoolHost _host;
        private readonly IFilePoolFactory _filePoolFactory;

        /// <summary>ctor
        /// </summary>
        public TrainFactory(ILogger<TrainFactory> logger, ISpoolHost host, IFilePoolFactory filePoolFactory)
        {
            _logger = logger;
            _host = host;
            _filePoolFactory = filePoolFactory;
        }

        /// <summary>获取路径下的序列
        /// </summary>
        public List<Train> GetTrainsFromPath(FilePoolOption option)
        {
            var trains = new List<Train>();
            var directoryInfo = new DirectoryInfo(option.Path);
            var subDirs = directoryInfo.GetDirectories();
            foreach (var subDir in subDirs)
            {
                //是否为序列的文件夹名
                if (TrainUtil.IsTrainName(subDir.Name))
                {
                    //序列索引
                    var index = TrainUtil.GetTrainIndex(subDir.Name);
                    var train = CreateTrain(option, index);
                    trains.Add(train);
                }
            }
            return trains;
        }

        /// <summary>创建序列
        /// </summary>
        public Train CreateTrain(FilePoolOption option, int index)
        {
            using (var scope = _host.Provider.CreateScope())
            {
                var trainOption = scope.ServiceProvider.GetService<TrainOption>();
                trainOption.Index = index;
                var train = scope.ServiceProvider.GetService<Train>();
                var filePoolOption = scope.ServiceProvider.GetService<FilePoolOption>();
                _filePoolFactory.SetScopeOption(filePoolOption, option);
                _logger.LogDebug("创建文件池:'{0}'下面的序列,序列号为:'{1}',", option.Name, index);
                return train;
            }
        }

        /// <summary>获取序列索引最新的序列
        /// </summary>
        public Train GetLatest(IEnumerable<Train> trains)
        {
            return trains.OrderBy(x => x.Index).LastOrDefault();
        }

        /// <summary>根据序列信息,文件池配置信息获取序列基本信息
        /// </summary>
        public TrainInfo BuildInfo(Train train, FilePoolOption option)
        {
            var info = new TrainInfo()
            {
                FilePoolName = option.Name,
                FilePoolPath = option.Path,
                Index = train.Index,
                Name = train.Name,
                Path = train.Path,
                TrainType = train.TrainType
            };
            return info;
        }



    }
}
