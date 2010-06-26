#region Usings

using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

#endregion

namespace helpmebot6
{
    internal class Twitter
    {
        public static void tweet(string status)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if ( !Helpmebot6.enableTwitter ) return;
            Twitter twit = new Twitter(
                Configuration.singleton().retrieveGlobalStringOption("twitterUsername"),
                Configuration.singleton().retrieveGlobalStringOption("twitterPassword")
                )
                               {
                                   userAgent =
                                       Configuration.singleton( ).
                                       retrieveGlobalStringOption( "useragent" )
                               };
            twit.statuses_update(status);
        }

        public Twitter(string username, string password)
        {
            twitterUsername = username;
            twitterPassword = password;
            userAgent = "Utility/0.1 (TwitterClient +http://svn.helpmebot.org.uk:3690/svn/utility)";
        }

        public string twitterUsername { get; set; }

        public string twitterPassword { private get; set; }

        public string userAgent { get; set; }

        /// <summary>
        ///   Updates the authenticating user's status.  Requires the status parameter specified below.  Request must be a POST.  A status update with text identical to the authenticating user's current status will be ignored to prevent duplicates.
        /// </summary>
        /// <param name = "status">The text of your status update. URL encode as necessary. Statuses over 140 characters will cause a 403 error to be returned from the API.</param>
        /// <see cref = "http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses%C2%A0update" />
// ReSharper disable InconsistentNaming
        public HttpStatusCode statuses_update(string status)
// ReSharper restore InconsistentNaming
        {
            // POST
            const string url = "http://api.twitter.com/1/statuses/update.xml";

            status = (status.Length > 140 ? status.Substring(0, 140) : status);
            string postData = "status=" + status;

            try
            {
                HttpWebResponse wrsp = postResponse(url, postData);
                return wrsp.StatusCode;
            }
            catch (WebException ex)
            {
                GlobalFunctions.errorLog(ex);
            }

            return HttpStatusCode.BadRequest;
        }

        private HttpWebResponse postResponse(string url, string parameters)
        {
            HttpWebRequest wr = (HttpWebRequest) WebRequest.Create(url);
            wr.Credentials = new NetworkCredential(twitterUsername, twitterPassword);
            wr.Method = "POST";
            wr.UserAgent = userAgent;


            wr.ContentType = "application/x-www-form-urlencoded";
            wr.ContentLength = parameters.Length;
            ServicePointManager.Expect100Continue = false;
            using (Stream writeStream = wr.GetRequestStream())
            {
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = encoding.GetBytes(parameters);
                writeStream.Write(bytes, 0, bytes.Length);
            }
            HttpWebResponse wrsp;
            try
            {
                wrsp = (HttpWebResponse) wr.GetResponse();
            }
            catch (WebException ex)
            {
                wrsp = (HttpWebResponse) ex.Response;
            }
            return wrsp;
        }
    }
}