namespace Helpmebot.Services.Interfaces
{
    using System.Collections.Specialized;
    using System.IO;

    public interface IWebServiceClient
    {
        Stream DoApiCall(NameValueCollection query, string endpoint, bool post = false);
    }
}