using System;
using System.IO;
using System.Threading.Tasks;
using Transfer.Core;

namespace Transfer.Datasource.Files
{
    public class FileWriter : IWriter
    {
        private readonly string _destinationPath;

        public FileWriter(string destinationPath)
        {
            _destinationPath = string.IsNullOrWhiteSpace(destinationPath)
                ? throw new ArgumentException("Destination path cannot be null, empty or whitespace.", nameof(destinationPath))
                : destinationPath;
        }

        public Task<Stream> GetDestinationStreamAsync()
        {
            var fullPath = Path.GetFullPath(_destinationPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            return Task.FromResult(File.Create(fullPath) as Stream);
        }
    }
}