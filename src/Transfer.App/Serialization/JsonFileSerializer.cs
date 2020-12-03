using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Transfer.App.Logging;

namespace Transfer.App.Serialization
{
    internal sealed record DeserializationResult<T>(bool IsSuccessful, T Data);

    internal class JsonFileSerializer<T>
    {
        private readonly ILog _logger;

        public JsonFileSerializer(ILog logger) =>
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> TrySerializeAsync(T data, string filePath)
        {
            try
            {
                using var stream = File.Create(filePath);
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true
                };

                await JsonSerializer.SerializeAsync<T>(stream, data, jsonOptions).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return false;
            }

            return true;
        }

        public async Task<DeserializationResult<T>> TryDeserializeAsync(string filePath)
        {
            T data = default;
            try
            {
                using var stream = File.OpenRead(filePath);
                var jsonOptions = new JsonSerializerOptions { IncludeFields = true };
                data = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return new DeserializationResult<T>(false, data);
            }

            return new DeserializationResult<T>(true, data);
        }
    }
}