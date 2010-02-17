using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace helpmebot6
{
    static class HttpRequest
    {
        public static Stream get( string uri )
        {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create( uri );
            hwr.UserAgent = Configuration.Singleton( ).retrieveGlobalStringOption( "useragent" );
            HttpWebResponse resp = (HttpWebResponse)hwr.GetResponse( );

            return resp.GetResponseStream( );
        }
    }
}
