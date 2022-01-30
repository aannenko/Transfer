using System.Net;

namespace Transfer.Datasource.Ftp;

internal static class FtpRequestRetriever
{
    public static FtpWebRequest GetRequest(string requestMethod, Uri filePath,
        NetworkCredential? credentials = null, IWebProxy? proxy = null)
    {
        var request = FtpWebRequest.CreateDefault(filePath) as FtpWebRequest;
        request!.Method = requestMethod;

        if (credentials != null)
            request.Credentials = credentials;

        if (proxy != null)
            request.Proxy = proxy;

        return request;
    }
}
