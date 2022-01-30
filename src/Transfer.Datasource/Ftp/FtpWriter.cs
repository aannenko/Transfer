using System.Net;
using Transfer.Core;

namespace Transfer.Datasource.Ftp
{
    public class FtpWriter : IWriter
    {
        private readonly Uri _filePath;
        private readonly NetworkCredential? _credentials;
        private readonly IWebProxy? _proxy;

        public FtpWriter(Uri filePath, NetworkCredential? credentials = null, IWebProxy? proxy = null)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _credentials = credentials;
            _proxy = proxy;
        }

        public async Task<Stream> GetDestinationStreamAsync()
        {
            return await FtpRequestRetriever.GetRequest(
                WebRequestMethods.Ftp.UploadFile, _filePath, _credentials, _proxy)
                    .GetRequestStreamAsync().ConfigureAwait(false);
        }
    }
}