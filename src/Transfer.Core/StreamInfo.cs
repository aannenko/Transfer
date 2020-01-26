using System;
using System.IO;

namespace Transfer.Core
{
    public class StreamInfo
    {
        public StreamInfo(Stream stream, double length = 0)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            Length = stream.CanSeek
                ? stream.Length
                : length > 0
                    ? length
                    : throw new ArgumentOutOfRangeException(nameof(length),
                        "The stream's length cannot be inferred from the stream, please provide a value.");
        }

        public Stream Stream { get; }

        public double Length { get; }
    }
}