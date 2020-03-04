using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spool.Scheduling;
using Spool.Utility;
using Spool.Writer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Spool.Group
{
    /// <summary>Group filePool,store files by group
    /// </summary>
    public class GroupPool
    {
        public bool IsRunning { get { return _isRunning == 1; } }
        private int _isRunning = 0;
        public string GroupName { get { return _descriptor.Name; } }
        public string GroupPath { get { return _descriptor.Path; } }

        private readonly ConcurrentQueue<SpoolFile> _spoolFileQueue;

        private readonly ConcurrentDictionary<string, SpoolFile> _processingFileDict;
        private List<Train> _trains;


        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;
        private readonly IScheduleService _scheduleService;

        private SpoolGroupDescriptor _descriptor;
        private readonly SpoolOption _option;
        private readonly IdGenerator _idGenerator;
        private readonly ITrainManager _trainManager;
        private readonly IFileWriterManager _fileWriterManager;


        /// <summary>Ctor
        /// </summary>
        public GroupPool(IServiceProvider provider, ILogger<GroupPool> logger, IScheduleService scheduleService, SpoolOption option, IdGenerator idGenerator, SpoolGroupDescriptor descriptor)
        {
            _provider = provider;
            _logger = logger;
            _scheduleService = scheduleService;

            _option = option;
            _idGenerator = idGenerator;
            _descriptor = descriptor;


            _spoolFileQueue = new ConcurrentQueue<SpoolFile>();
            _processingFileDict = new ConcurrentDictionary<string, SpoolFile>();
            _trains = new List<Train>();


            using (var scope = _provider.CreateScope())
            {
                var groupPoolDescriptor = scope.ServiceProvider.GetService<SpoolGroupDescriptor>();
                groupPoolDescriptor.Name = descriptor.Name;
                groupPoolDescriptor.Path = descriptor.Path;
                _trainManager = scope.ServiceProvider.GetService<ITrainManager>();

                var fileWriterOption = scope.ServiceProvider.GetService<FileWriterOption>();
                fileWriterOption.GroupName = _descriptor.Name;
                fileWriterOption.MaxFileWriterCount = _option.GroupMaxFileWriterCount;
                _fileWriterManager = scope.ServiceProvider.GetService<IFileWriterManager>();
            }
        }

        public void Initialize()
        {
            if (_fileWriterManager == null)
            {
                _logger.LogError("GroupPool fileWriterManager is null,please check the code, groupName is '{0}'.", GroupName);
            }
            if (DirectoryHelper.CreateIfNotExists(GroupPath))
            {
                _logger.LogInformation("GroupPool create group '{0}' , groupPath '{1}'.", GroupName, GroupPath);
            }
            //FileWiterManager Initialize
            _fileWriterManager.Initialize();

            _trains = _trainManager.FindTrains();
        }

        /// <summary>Start
        /// </summary>
        public void Start()
        {
            if (_isRunning == 1)
            {
                _logger.LogWarning("GroupPool is in running, GroupName '{0}',GroupPath '{1}'.", GroupName, GroupPath);
                return;
            }
            Interlocked.Exchange(ref _isRunning, 1);
        }

        /// <summary>Shutdown the groupPool
        /// </summary>
        public void Shutdown()
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }

        /// <summary>Write file to real path
        /// </summary>
        /// <param name="stream">File stream</param>
        /// <param name="ext">File extension</param>
        /// <returns></returns>
        private Task<SpoolFile> WriteFileInternal(Stream stream, string ext)
        {
            return Task.Run<SpoolFile>(() =>
            {
                var fileWriter = _fileWriterManager.Get();
                var fileName = GenerateFileName(ext);
                var savePath = Path.Combine(GroupPath, fileName);
                try
                {
                    //WriteFile
                    fileWriter.WriteFileAsync(stream, savePath).Wait();
                    var spoolFile = new SpoolFile()
                    {
                        FileName = fileName,
                        Path = savePath,
                        Ext = ext
                    };
                    return spoolFile;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Write file has exception,{0}", ex.Message);
                    throw;
                }
                finally
                {
                    _logger.LogDebug("Return fileWriter from groupPool '{0}'.", GroupName);
                    _fileWriterManager.Return(fileWriter);
                }
            });
        }

        private string GenerateFileName(string ext)
        {
            return $"{_idGenerator.GenerateId()}{ext}";
        }







    }
}
