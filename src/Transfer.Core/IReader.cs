using System.Threading.Tasks;

namespace Transfer.Core
{
    public interface IReader
    {
        Task<StreamInfo> GetSourceStreamInfoAsync();
    }
}