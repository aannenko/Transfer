using System;
using System.IO;
using System.Threading.Tasks;
using Transfer.Core;

namespace Transfer.Datasource.Files
{
    public class FileReader : IReader
    {
        private readonly string _sourcePath;

        public FileReader(string sourcePath)
        {
            _sourcePath = string.IsNullOrWhiteSpace(sourcePath)
                ? throw new ArgumentException("Source path cannot be null, empty or whitespace.", nameof(sourcePath))
                : sourcePath;
        }

        public Task<StreamInfo> GetSourceStreamInfoAsync()
        {
            if (!File.Exists(_sourcePath))
                throw new ArgumentException($"File {_sourcePath} does not exist.", nameof(_sourcePath));

            var stream = File.OpenRead(_sourcePath);
            return Task.FromResult(new StreamInfo(stream));
        }
    }
}