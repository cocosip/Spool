# Spool

[![996.icu](https://img.shields.io/badge/link-996.icu-red.svg)](https://996.icu) [![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/cocosip/Spool/blob/master/LICENSE) ![GitHub last commit](https://img.shields.io/github/last-commit/cocosip/Spool.svg) ![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/cocosip/Spool.svg)

| Build Server | Platform | Build Status |
| ------------ | -------- | ------------ |
| Azure Pipelines| Windows |[![Build Status](https://dev.azure.com/cocosip/Spool/_apis/build/status/cocosip.Spool?branchName=master&jobName=Windows)](https://dev.azure.com/cocosip/Spool/_build/latest?definitionId=8&branchName=master)|
| Azure Pipelines| Linux |[![Build Status](https://dev.azure.com/cocosip/Spool/_apis/build/status/cocosip.Spool?branchName=master&jobName=Linux)](https://dev.azure.com/cocosip/Spool/_build/latest?definitionId=8&branchName=master)|

| Package  | Version | Downloads|
| -------- | ------- | -------- |
| `Spool` | [![NuGet](https://img.shields.io/nuget/v/Spool.svg)](https://www.nuget.org/packages/Spool) |![NuGet](https://img.shields.io/nuget/dt/Spool.svg)|

## Spool是一个支持多个组的文件池

> 将文件保存到文件池中,在需要的时候取出文件。

## Features

- 支持多线程并发读写
- 支持多个文件池在同一个程序中启动
- 支持文件使用后主动释放,主动归还的功能
- 支持应用程序自动归还,对文件未释放或者未归还的文件,应用程序可以自动归还
- 支持文件池监控指定的目录,当目录下有文件时自动写入到文件池
- 支持文件获取数量,文件写入线程数的控制
- 支持在依赖注入后(程序启动前)修改线程池配置参数

## Samples

> 初始化代码

```c#
var descriptor1 = new FilePoolDescriptor()
{
    Name = "pool1",
    Path = "D:\\pool1",
    MaxFileWriterCount = 10,
    TrainMaxFileCount = 65535,
    WriteBufferSize = 1024 * 1024 * 2,
    EnableAutoReturn = true,
    AutoReturnSeconds = 5,
    ScanReturnFileMillSeconds = 1000,
    EnableFileWatcher = true,
    FileWatcherPath = "D:\\watcher1",
    ScanFileWatcherMillSeconds = 5000,
};

IServiceCollection services = new ServiceCollection();
services.AddLogging(l =>
{
    l.AddConsole().SetMinimumLevel(LogLevel.Debug);
}).AddSpool(o =>
{
    o.DefaultPool = "pool1";
    o.FilePools = new List<FilePoolDescriptor>()
    {
        descriptor1
    };
});

Provider = services.BuildServiceProvider();
Provider.UseSpool();

```

> 获取文件

```c#
var spoolFiles = spoolPool.Get("pool1", 50);

```

> 写入文件

```c#
var spoolFile = await spoolPool.WriteAsync("pool1","D:\\file1.text");

```

> 释放文件(释放文件后文件将会被删除)

```c#
spoolPool.Return("pool1", spoolFiles);
```

> 归还文件(当某一批文件处理失败后,需要放回文件池)

```c#
spoolPool.Release("pool1", spoolFiles);
```
