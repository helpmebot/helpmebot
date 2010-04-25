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
            string[] wc = {"suc_fullurl = '" + longUrl.ToString() + "'"};
            string cachelookup = DAL.Singleton().Select("suc_shorturl", "shorturlcache",null,wc,null,null,null,0,0) ;

            if( cachelookup == "" )
            {

                HttpWebRequest wrq = (HttpWebRequest)WebRequest.Create( "http://is.gd/api.php?longurl=" + longUrl.ToString( ) );
                wrq.UserAgent = Configuration.Singleton( ).retrieveGlobalStringOption( "useragent" );
                HttpWebResponse wrs = (HttpWebResponse)wrq.GetResponse( );
                if( wrs.StatusCode == HttpStatusCode.OK )
                {
                    StreamReader sr = new StreamReader( wrs.GetResponseStream( ) );
                    string shorturl = sr.ReadLine( );
                    DAL.Singleton( ).ExecuteNonQuery( "INSERT INTO shorturlcache VALUES (null, '" + longUrl + "', '" + shorturl + "');" );
                    return new Uri( shorturl );
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new Uri( cachelookup );
            }
        }
    }
}
