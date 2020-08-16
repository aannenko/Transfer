using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Transfer.Core
{
    internal class Transfer
    {
        private const int _bufferSize = 32768;

        private static readonly BoundedChannelOptions _channelOptions = new BoundedChannelOptions(20)
        {
            SingleReader = true,
            SingleWriter = true
        };

        private readonly IReader _reader;
        private readonly IWriter _writer;

        public Transfer(IReader reader, IWriter writer)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public async Task TransferDataAsync(IProgress<double> progress = null, CancellationToken token = default)
        {
            var channel = Channel.CreateBounded<(IMemoryOwner<byte>, int)>(_channelOptions);

            var source = await _reader.GetSourceStreamInfoAsync();
            using (source.Stream)
            using (var destStream = await _writer.GetDestinationStreamAsync())
            {
                var reading = ReadAllAsync(channel.Reader, destStream, source.Length, progress, token)
                    .ConfigureAwait(false);

                int read;
                IMemoryOwner<byte> owner;
                while ((read = await source.Stream.ReadAsync(
                    (owner = MemoryPool<byte>.Shared.Rent(_bufferSize)).Memory, token).ConfigureAwait(false)) > 0)
                        await channel.Writer.WriteAsync((owner, read), token).ConfigureAwait(false);

                channel.Writer.Complete();
                await reading;
            }
        }

        private async Task ReadAllAsync(ChannelReader<(IMemoryOwner<byte>, int)> reader,
            Stream destStream, double sourceLength, IProgress<double> progress, CancellationToken token)
        {
            double totalRead = 0;
            while (await reader.WaitToReadAsync(token).ConfigureAwait(false))
                while (reader.TryRead(out var pair))
                {
                    var (owner, read) = pair;
                    using (owner)
                    {
                        var memory = owner.Memory.Length == read
                            ? owner.Memory
                            : owner.Memory.Slice(0, read);

                        await destStream.WriteAsync(memory, token).ConfigureAwait(false);
                    }

                    progress?.Report((totalRead += read) / sourceLength);
                }
        }
    }
}