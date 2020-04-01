﻿using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Spool.Writers
{
    /// <summary>文件写入器
    /// </summary>
    public class FileWriter
    {
        public string Id { get; }
        private readonly ILogger _logger;
        private readonly FilePoolOption _option;

        public FileWriter(ILogger<FileWriter> logger, FilePoolOption option)
        {
            _logger = logger;
            _option = option;
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>写入文件
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="targetPath">存储路径</param>
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
                    byte[] buffers = new byte[_option.WriteBufferSize];
                    //读取一次
                    int r = stream.Read(buffers, 0, buffers.Length);
                    //判断本次是否读取到了数据
                    while (r > 0)
                    {
                        fw.Write(buffers, 0, r);
                        r = stream.Read(buffers, 0, buffers.Length);
                    }
                }
            });
        }
    }
}