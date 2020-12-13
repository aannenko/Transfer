using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Transfer.App.Serialization
{
    internal class JsonFileSerializer<T>
    {
        public async Task SerializeAsync(T data, string filePath,
            CancellationToken cancellationToken = default)
        {
            using var stream = File.Create(filePath);
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            await JsonSerializer.SerializeAsync<T>(stream, data, jsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<T> DeserializeAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using var stream = File.OpenRead(filePath);
            var jsonOptions = new JsonSerializerOptions { IncludeFields = true };
            return await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}