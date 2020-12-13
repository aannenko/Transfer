using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Transfer.App.Logging;
using Transfer.App.Serialization;
using Transfer.App.Tools;
using Transfer.Core;
using Transfer.Datasource.Files;
using Transfer.Datasource.Ftp;

var logger = new ConsoleLogger();
var serializer = new JsonFileSerializer<Data>();
var cancellation = new CancellationTokenSource();
var dataFilePath = Path.GetFullPath("Transfers.json");
var sampleData = new Lazy<Data>(() => new Data
{
    Files = new[]
    {
        ("ftp://ftp.uconn.edu/48_hour/file1.txt", @"C:\Downloads\file1.txt"),
        ("ftp://ftp.uconn.edu/48_hour/file2.txt", @"C:\Downloads\file2.txt")
    },
    UserName = "anonymous",
    Password = "some@email.com",
    Proxy = "http://optional.proxy"
});

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    cancellation.Cancel();
};

Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

Data data = await ReadDataAsync();
if (data is not null && TryConvertDataToTransferInfo(data, out List<TransferInfo> info))
{
    logger.Info($"Initializing transfers for {info.Count} files.");
    await TransferAsync(info);
    logger.Info("Transfer complete.");
}
else if (!cancellation.IsCancellationRequested)
    await WriteSampleDataAsync();

if (Environment.UserInteractive)
{
    logger.Info("Press any key to exit...");
    Console.ReadKey();
}

async Task<Data> ReadDataAsync()
{
    logger.Info($"Reading '{dataFilePath}' file.");
    try
    {
        return await serializer.DeserializeAsync(dataFilePath, cancellation.Token);
    }
    catch (OperationCanceledException)
    {
        logger.Warn($"Reading file '{dataFilePath}' cancelled.");
    }
    catch (Exception e)
    {
        logger.Warn($"File '{dataFilePath}' not found or invalid: {e.Message}");
    }

    return null;
}

async Task WriteSampleDataAsync()
{
    logger.Info($"Creating file '{dataFilePath}' with sample transfer details; please fill it with proper details.");
    try
    {
        await serializer.SerializeAsync(sampleData.Value, dataFilePath, cancellation.Token);
    }
    catch (OperationCanceledException)
    {
        logger.Warn($"Writing file '{dataFilePath}' cancelled.'");
    }
    catch (Exception e)
    {
        logger.Error($"File '{dataFilePath}' cannot be written: {e.Message}");
    }
}

bool TryConvertDataToTransferInfo(Data data, out List<TransferInfo> info)
{
    info = new List<TransferInfo>();
    if (data?.Files == null || data.Files.Any(f => f.Source == null || f.Destination == null))
        return false;

    var registry = BuildRegistry();
    bool dataContainsErrors = false;
    foreach (var file in data.Files)
    {
        try
        {
            info.Add(registry.GetTransferInfo(file.Source, file.Destination,
                $"from '{file.Source}' to '{file.Destination}'", new Progress<double>(), cancellation.Token));
        }
        catch (Exception e)
        {
            dataContainsErrors = true;
            logger.Error(e.Message);
        }
    }

    return !dataContainsErrors;
}

static ReaderWriterRegistry BuildRegistry()
{
    var registry = new ReaderWriterRegistry();

    registry.RegisterReader<string>(_ => true, path => new FileReader(path));
    registry.RegisterWriter<string>(_ => true, path => new FileWriter(path));

    registry.RegisterReader<string>(
        path => path.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase),
        path => new FtpReader(new Uri(path), null, null));

    registry.RegisterWriter<string>(
        path => path.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase),
        path => new FtpWriter(new Uri(path), null, null));

    return registry;
}

async Task TransferAsync(IEnumerable<TransferInfo> info)
{
    var getSpinnerTask = AsyncSpinner.GetSpinnerAsync();
    var manager = new TransferManager(Environment.ProcessorCount);

    var sWatch = Stopwatch.StartNew();

    int transfersStarted = 0;
    var transferTask = manager.TransferDataAsync(info, async (task, transfer) =>
    {
        var transferNumber = Interlocked.Increment(ref transfersStarted);
        try
        {
            logger.Info($"Starting transfer {transferNumber} {transfer.Description}.");
            await task;
            logger.Info($"Transfer {transferNumber} {transfer.Description} complete.");
        }
        catch (OperationCanceledException)
        {
            logger.Warn($"Transfer {transferNumber} {transfer.Description} cancelled.");
        }
        catch (Exception e)
        {
            logger.Error($"Transfer {transferNumber} {transfer.Description} failed." +
                $"{Environment.NewLine}Error message: {Environment.NewLine + e.Message}");
        }
    });

    var spinningTask = (await getSpinnerTask).SpinUntilAsync(transferTask);
    await Task.WhenAll(spinningTask, transferTask);

    sWatch.Stop();

    logger.Info($"Transfers took {sWatch.Elapsed}.");
}
