using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Spool.Writer
{
    /// <summary>Write source file to spool,source file can be a path,or a file stream
    /// </summary>
    public class FileWriter
    {
        public string Id { get; }
        private readonly ILogger _logger;

        /// <summary>Ctor
        /// </summary>
        public FileWriter(ILogger<FileWriter> logger)
        {
            _logger = logger;
            Id = Guid.NewGuid().ToString("N");
        }

        /// <summary>Write source path to target path
        /// </summary>
        /// <param name="sourcePath">Source path</param>
        /// <param name="targetPath">Target path</param>
        /// <returns></returns>
        public Task WriteFileAsync(string sourcePath, string targetPath)
        {
            using (var stream = File.OpenRead(sourcePath))
            {
                return WriteFileInternal(stream, targetPath);
            }
        }

        /// <summary>Write stream to target path
        /// </summary>
        /// <param name="stream">Source stream</param>
        /// <param name="targetPath">Target path</param>
        /// <returns></returns>
        public Task WriteFileAsync(Stream stream, string targetPath)
        {
            return WriteFileInternal(stream, targetPath);
        } 

        private Task WriteFileInternal(Stream stream, string targetPath)
        {
            return Task.Run(() =>
            {
                using (FileStream fw = File.OpenWrite(targetPath))
                {
                    //设置缓冲区大小
                    byte[] buffers = new byte[1024 * 1024 * 5];
                    //读取一次
                    int r = stream.Read(buffers, 0, buffers.Length);
                    //判断本次是否读取到了数据
                    while (r > 0)
                    {
                        fw.Write(buffers, 0, r);
                    }
                }
            });
        }


        public override string ToString()
        {
            return $"FileWriter, [Id:{Id}]";
        }


    }
}
