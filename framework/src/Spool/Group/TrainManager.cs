using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Spool.Group
{
    public class TrainManager : ITrainManager
    {
        private readonly ILogger _logger;
        private readonly GroupPoolDescriptor _descriptor;

        /// <summary>Ctor
        /// </summary>
        public TrainManager(ILogger<TrainManager> logger, GroupPoolDescriptor descriptor)
        {
            _logger = logger;
            _descriptor = descriptor;
        }

        //public List<Train> FindTrains()
        //{
        //    var trains = new List<Train>();
        //    var groupDirectoryInfo = new DirectoryInfo(_descriptor.GroupPath);
        //    var trainDirectoryInfos = groupDirectoryInfo.GetDirectories();
        //    foreach (var trainDirectoryInfo in trainDirectoryInfos)
        //    {
        //        if (IsTrainName(trainDirectoryInfo.Name))
        //        {
        //            trains.Add(new Train())
        //        }
        //    }

        //}



        public bool IsTrainName(string name)
        {
            var pattern = "^\\_[0-9]{5}\\_$";
            return Regex.IsMatch(name, pattern);
        }

        /// <summary>Get train index from name
        /// </summary>
        public int GetTrainIndex(string name)
        {
            if (int.TryParse(name.Replace('_', ' '), out int r))
            {
                return r;
            }
            return 0;
        }


    }
}
