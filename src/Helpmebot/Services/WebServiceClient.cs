namespace Helpmebot.Services
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using Helpmebot.Services.Interfaces;

    public class WebServiceClient : IWebServiceClient
    {
        private readonly string userAgent;
        private readonly ILogger logger;

        private readonly object lockObject = new object();
        private readonly CookieContainer cookieJar = new CookieContainer();

        public WebServiceClient(BotConfiguration botConfiguration, ILogger logger)
        {
            this.logger = logger;
            this.userAgent = botConfiguration.UserAgent;
        }

        public Stream DoApiCall(NameValueCollection query, string endpoint, bool post = false)
        {
            query.Set("format", "xml");

            var queryFragment = string.Join("&", query.AllKeys.Select(a => a + "=" + WebUtility.UrlEncode(query[a])));

            var url = endpoint;

            if (!post)
            {
                url = string.Format("{0}?{1}", endpoint, queryFragment);
            }

            this.logger.DebugFormat("Requesting {0}", url);

            var hwr = (HttpWebRequest) WebRequest.Create(url);
            hwr.CookieContainer = this.cookieJar;
            hwr.UserAgent = this.userAgent;
            hwr.Method = post ? "POST" : "GET";

            if (post)
            {
                hwr.ContentType = "application/x-www-form-urlencoded";
                hwr.ContentLength = queryFragment.Length;

                using (var requestWriter = new StreamWriter(hwr.GetRequestStream()))
                {
                    requestWriter.Write(queryFragment);
                    requestWriter.Flush();
                }
            }
        
            var memstream = new MemoryStream();

            lock (this.lockObject)
            {
                using (var resp = (HttpWebResponse) hwr.GetResponse())
                {
                    this.cookieJar.Add(resp.Cookies);
                    
                    var responseStream = resp.GetResponseStream();

                    if (responseStream == null)
                    {
                        throw new NullReferenceException("Returned web request response stream was null.");
                    }

                    responseStream.CopyTo(memstream);
                    resp.Close();
                }
            }

            memstream.Position = 0;
            return memstream;
        }
    }
}