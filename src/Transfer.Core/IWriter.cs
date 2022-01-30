namespace Transfer.Core;

public interface IWriter
{
    Task<Stream> GetDestinationStreamAsync();
}
