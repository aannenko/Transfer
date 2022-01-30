using System.Net;
using Transfer.Core;

namespace Transfer.Datasource.Ftp;

public class FtpReader : IReader
{
    private readonly Uri _filePath;
    private readonly NetworkCredential? _credentials;
    private readonly IWebProxy? _proxy;

    public FtpReader(Uri filePath, NetworkCredential? credentials = null, IWebProxy? proxy = null)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _credentials = credentials;
        _proxy = proxy;
    }

    public async Task<StreamInfo> GetSourceStreamInfoAsync()
    {
        var response = await FtpRequestRetriever.GetRequest(
            WebRequestMethods.Ftp.DownloadFile, _filePath, _credentials, _proxy)
                .GetResponseAsync().ConfigureAwait(false);

        return new StreamInfo(response.GetResponseStream(), response.ContentLength);
    }
}
