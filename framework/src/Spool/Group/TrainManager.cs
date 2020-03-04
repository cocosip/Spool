﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Spool.Group
{
    public class TrainManager : ITrainManager
    {
        private List<Train> _trains;

        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;
        private readonly SpoolGroupDescriptor _descriptor;

        /// <summary>Ctor
        /// </summary>
        public TrainManager(IServiceProvider provider, ILogger<TrainManager> logger, SpoolGroupDescriptor descriptor)
        {
            _provider = provider;
            _logger = logger;
            _descriptor = descriptor;

            _trains = new List<Train>();
        }

        /// <summary>Find trains
        /// </summary>
        public List<Train> FindTrains()
        {
            return _trains;
        }


        /// <summary>Whether the name is a Train's name
        /// </summary>
        public bool IsTrainName(string name)
        {
            var pattern = @"^_[0-9]{6}_$";
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

        /// <summary>Create train by group
        /// </summary>
        public Train CreateTrain(SpoolGroupDescriptor descriptor, int index)
        {
            var train = (Train)ActivatorUtilities.CreateInstance(_provider, typeof(Train), descriptor, index);
            return train;
        }


    }
}
