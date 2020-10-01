using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Transfer.App.Logging;
using Transfer.App.Serialization;
using Transfer.App.Tools;
using Transfer.Core;
using Transfer.Datasource.Files;
using Transfer.Datasource.Ftp;

namespace Transfer.App
{
    class Program
    {
        private const string DataFileName = "Transfers.xml";

        private static readonly ILog _logger = new ConsoleLogger();
        private static readonly XmlFileSerializer<Data> _serializer = new XmlFileSerializer<Data>();

        private static readonly Lazy<Data> _sampleData = new Lazy<Data>(() => new Data
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

        static async Task Main()
        {
            _logger.Info($"Looking for '{DataFileName}' file.");
            if (_serializer.TryDeserialize(DataFileName, out Data data) && TryConvertDataToTransferInfo(data, out List<TransferInfo> info))
            {
                _logger.Info($"Initializing transfers for {data.Files.Length} files.");
                await Transfer(info);
                _logger.Info("Transfer complete.");
            }
            else
            {
                _logger.Warn($"'{DataFileName}' file not found or invalid.");
                _logger.Info($"Creating file '{DataFileName}' with sample transfer details; please fill it with proper details.");
                if (!_serializer.TrySerialize(_sampleData.Value, DataFileName))
                    _logger.Error($"Cannot create file '{DataFileName}' in the application folder.");
            }

            _logger.Info("Press any key to exit...");
            Console.ReadKey();
        }

        private static bool TryConvertDataToTransferInfo(Data data, out List<TransferInfo> info)
        {
            info = new List<TransferInfo>();
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
                    _logger.Error(e.Message);
                }
            }

            return !dataContainsErrors;
        }

        private static ReaderWriterRegistry BuildRegistry()
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

        private static async Task Transfer(IEnumerable<TransferInfo> info)
        {
            var getSpinnerTask = Spinner.GetSpinnerAsync();
            var client = new TransferManager(Environment.ProcessorCount);

            int transfersStarted = 0;
            var transferTask = client.TransferDataAsync(info, async (task, transfer) =>
            {
                var transferNumber = Interlocked.Increment(ref transfersStarted);
                try
                {
                    _logger.Info($"Starting transfer {transferNumber} {transfer.Description}.");
                    await task;
                    _logger.Info($"Transfer {transferNumber} {transfer.Description} complete.");
                }
                catch (OperationCanceledException)
                {
                    _logger.Warn($"Transfer {transferNumber} {transfer.Description} cancelled.");
                }
                catch (Exception e)
                {
                    _logger.Error($"Transfer {transferNumber} {transfer.Description} failed." +
                        $"{Environment.NewLine}Error message: {Environment.NewLine + e.Message}");
                }
            });

            var spinningTask = (await getSpinnerTask).SpinUntilAsync(transferTask);

            var sWatch = Stopwatch.StartNew();
            await Task.WhenAll(spinningTask, transferTask);
            sWatch.Stop();

            _logger.Info($"Transfer took {sWatch.Elapsed}.");
        }
    }
}
