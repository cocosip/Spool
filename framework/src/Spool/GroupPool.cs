using Microsoft.Extensions.Logging;
using Spool.Writer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Spool.Dependency;
using System.IO;
using Spool.Utility;
using System.Collections.Concurrent;

namespace Spool
{
    /// <summary>Group filePool,store files by group
    /// </summary>
    public class GroupPool
    {
        public string GroupName { get; }

        public string GroupPath { get; }

        private ConcurrentDictionary<string, SpoolFile> _spoolFileDict;

        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;
        private readonly SpoolOption _option;
        private readonly IdGenerator _idGenerator;
        private readonly FileWriterManager _fileWriterManager;


        /// <summary>Ctor
        /// </summary>
        public GroupPool(IServiceProvider provider, ILogger<GroupPool> logger, SpoolOption option, IdGenerator idGenerator, string groupName)
        {
            _provider = provider;
            _logger = logger;
            _option = option;
            _idGenerator = idGenerator;

            _spoolFileDict = new ConcurrentDictionary<string, SpoolFile>();

            GroupName = groupName;
            GroupPath = Path.Combine(_option.RootPath, $"{Path.AltDirectorySeparatorChar}{GroupName}");
            _fileWriterManager = provider.CreateInstance<FileWriterManager>(groupName);

        }

        public void Initialize()
        {
            if (_fileWriterManager == null)
            {
                _logger.LogError("GroupPool fileWriterManager is null,please check the code, groupName is '{0}'.", GroupName);
            }
            if (DirectoryHelper.CreateIfNotExists(GroupPath))
            {
                _logger.LogInformation("GroupPool create groupPath '{0}'.", GroupPath);
            }
            //ReadAllFiles from files

        }






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
