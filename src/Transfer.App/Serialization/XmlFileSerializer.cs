using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Transfer.App.Serialization
{
    internal class XmlFileSerializer<T>
    {
        private readonly XmlSerializer _serializer =
            new XmlSerializer(typeof(T));

        public bool TrySerialize(T data, string filePath)
        {
            if (data == null)
                return false;

            using (var stream = File.Create(filePath))
            using (var writer = XmlWriter.Create(stream))
            {
                _serializer.Serialize(writer, data);
                return true;
            }
        }

        public bool TryDeserialize(string filePath, out T data)
        {
            data = default(T);

            if (!File.Exists(filePath))
                return false;

            using (var stream = File.OpenRead(filePath))
            using (var reader = XmlReader.Create(stream))
            {
                if (!_serializer.CanDeserialize(reader))
                    return false;

                data = (T)_serializer.Deserialize(reader);
                return true;
            }
        }
    }
}