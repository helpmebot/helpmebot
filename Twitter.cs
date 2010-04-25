using System.IO;
using System.Net;
using System.Text;

namespace helpmebot6
{
    class Twitter
    {
        public static void tweet(string status)
        {
           Twitter twit = new Twitter(
                    Configuration.Singleton( ).retrieveGlobalStringOption( "twitterUsername" ),
                    Configuration.Singleton( ).retrieveGlobalStringOption( "twitterPassword" )
                );
           twit.userAgent = Configuration.Singleton( ).retrieveGlobalStringOption( "useragent" );
           twit.statuses_update( status );
        }

        public Twitter( string username, string password )
        {
            _username = username;
            _password = password;
            _ua = "Utility/0.1 (TwitterClient +http://svn.helpmebot.org.uk:3690/svn/utility)";
        }

        string _username, _password, _ua;

        public string twitterUsername
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
            }
        }
        public string twitterPassword
        {
            private get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }

        public string userAgent
        {
            get
            {
                return _ua;
            }
            set
            {
                _ua = value;
            }
        }

        /// <summary>
        /// Updates the authenticating user's status.  Requires the status parameter specified below.  Request must be a POST.  A status update with text identical to the authenticating user's current status will be ignored to prevent duplicates.
        /// </summary>
        /// <param name="status">The text of your status update. URL encode as necessary. Statuses over 140 characters will cause a 403 error to be returned from the API.</param>
        /// <see cref="http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses%C2%A0update"/>
        public HttpStatusCode statuses_update( string status )
        { // POST
            string url = "http://api.twitter.com/1/statuses/update.xml";

            status = ( status.Length > 140 ? status.Substring( 0, 140 ) : status );
            string postData = "status=" + status;

            HttpWebResponse wrsp = postResponse( url, postData );

            return wrsp.StatusCode;
        }

        private HttpWebResponse postResponse( string url, string parameters )
        {
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create( url );
            wr.Credentials = new NetworkCredential( twitterUsername, twitterPassword );
            wr.Method = "POST";
            wr.UserAgent = this.userAgent;


            wr.ContentType = "application/x-www-form-urlencoded";
            wr.ContentLength = parameters.Length;
            System.Net.ServicePointManager.Expect100Continue = false;
            using( Stream writeStream = wr.GetRequestStream( ) )
            {
                UTF8Encoding encoding = new UTF8Encoding( );
                byte[ ] bytes = encoding.GetBytes( parameters );
                writeStream.Write( bytes, 0, bytes.Length );
            }

            HttpWebResponse wrsp = (HttpWebResponse)wr.GetResponse( );

            return wrsp;
        }

    }
}
