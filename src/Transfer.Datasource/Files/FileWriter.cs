using Transfer.Core;

namespace Transfer.Datasource.Files;

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
        var directoryName = Path.GetDirectoryName(_destinationPath);
        if (!string.IsNullOrEmpty(directoryName))
            Directory.CreateDirectory(directoryName);

        return Task.FromResult(File.Create(_destinationPath) as Stream);
    }
}
