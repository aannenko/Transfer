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
                : Path.GetFullPath(destinationPath);
        }

        public Task<Stream> GetDestinationStreamAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_destinationPath));
            return Task.FromResult(File.Create(_destinationPath) as Stream);
        }
    }
}