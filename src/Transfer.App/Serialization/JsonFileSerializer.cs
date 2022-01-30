using System.Text.Json;

namespace Transfer.App.Serialization
{
    internal class JsonFileSerializer<T>
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        public async Task SerializeAsync(T data, string filePath, CancellationToken cancellationToken = default)
        {
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, data, _jsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<T?> DeserializeAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}