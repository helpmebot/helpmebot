#region Usings

using System.IO;
using System.Net;
using System.Reflection;

#endregion

namespace helpmebot6
{
    internal static class HttpRequest
    {
        public static Stream get(string uri)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            HttpWebRequest hwr = (HttpWebRequest) WebRequest.Create(uri);
            hwr.UserAgent = Configuration.singleton().retrieveGlobalStringOption("useragent");
            HttpWebResponse resp = (HttpWebResponse) hwr.GetResponse();

            return resp.GetResponseStream();
        }
    }
}