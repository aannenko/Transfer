using System.IO;
using System.Threading.Tasks;

namespace Transfer.Core
{
    public interface IWriter
    {
        Task<Stream> GetDestinationStreamAsync();
    }
}