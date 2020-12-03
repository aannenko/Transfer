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

ILog logger = new ConsoleLogger();
JsonFileSerializer<Data> serializer = new JsonFileSerializer<Data>(logger);
string dataFilePath = Path.GetFullPath("Transfers.json");
Lazy<Data> sampleData = new Lazy<Data>(() => new Data
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

logger.Info($"Looking for '{dataFilePath}' file.");
var deserializationResult = await serializer.TryDeserializeAsync(dataFilePath);
if (deserializationResult.IsSuccessful && TryConvertDataToTransferInfo(deserializationResult.Data, out List<TransferInfo> info))
{
    logger.Info($"Initializing transfers for {info.Count} files.");
    await Transfer(info);
    logger.Info("Transfer complete.");
}
else
{
    logger.Warn($"'{dataFilePath}' file not found or invalid.");
    logger.Info($"Creating file '{dataFilePath}' with sample transfer details; please fill it with proper details.");
    if (!(await serializer.TrySerializeAsync(sampleData.Value, dataFilePath)))
        logger.Error($"Cannot create file '{dataFilePath}'.");
}

logger.Info("Press any key to exit...");
Console.ReadKey();

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
                $"from '{file.Source}' to '{file.Destination}'", new Progress<double>()));
        }
        catch (Exception e)
        {
            dataContainsErrors = true;
            logger.Error(e.Message);
        }
    }

    return !dataContainsErrors;
}

async Task Transfer(IEnumerable<TransferInfo> info)
{
    var getSpinnerTask = Spinner.GetSpinnerAsync();
    var client = new TransferManager(Environment.ProcessorCount);

    int transfersStarted = 0;
    var transferTask = client.TransferDataAsync(info, async (task, transfer) =>
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

    var sWatch = Stopwatch.StartNew();
    await Task.WhenAll(spinningTask, transferTask);
    sWatch.Stop();

    logger.Info($"Transfer took {sWatch.Elapsed}.");
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
