using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace helpmebot6
{
    class IsGd
    {
        public static Uri shorten( Uri longUrl )
        {
            HttpWebRequest wrq = (HttpWebRequest)WebRequest.Create( "http://is.gd/api.php?longurl=" + longUrl.ToString( ) );
            wrq.UserAgent = Configuration.Singleton( ).retrieveGlobalStringOption( "useragent" );
            HttpWebResponse wrs = (HttpWebResponse)wrq.GetResponse( );
            if( wrs.StatusCode == HttpStatusCode.OK )
            {
                StreamReader sr = new StreamReader( wrs.GetResponseStream( ) );
                return new Uri( sr.ReadLine( ) );
            }
            else
            {
                return null;
            }
        }
    }
}
