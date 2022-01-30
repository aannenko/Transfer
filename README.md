[![Build](https://github.com/aannenko/Transfer/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/aannenko/Transfer/actions/workflows/dotnetcore.yml)

# Transfer
Use code in this repository to build and run .NET Core console application that is able to efficiently transfer data from one place to the other with a configurable level of concurrency.

### Contents
The repository contains source code for three projects:
- [Transfer.App](https://github.com/aannenko/Transfer/tree/master/src/Transfer.App) is a console appliction that reads transfer settings and starts the transfer
- [Transfer.Core](https://github.com/aannenko/Transfer/tree/master/src/Transfer.Core) contains transfer management logic and holds abstractions for data readers and writers
- [Transfer.Datasource](https://github.com/aannenko/Transfer/tree/master/src/Transfer.Datasource) describes specific data readers and writers

### Usage
You will need [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) installed.

`cd` to the Transfer.App directory, run `dotnet run` and follow the instructions in the output.

### Remarks
The application uses a notion of reader/writer for data transfers; each transfer requires one reader and one writer. Readers and writers work with a type [`Stream`](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream) (readers produce streams, writers consume them) so in theory you could create readers and writers for any stream-compatible data and try combining them in the most crazy ways you can imagine. There's no guarantee that such transfers will succeed though. In the meantime, you can find a reader and a writer for local files and also a reader and a writer for files located on FTP in the project [Transfer.Datasource](https://github.com/aannenko/Transfer/tree/master/src/Transfer.Datasource).
