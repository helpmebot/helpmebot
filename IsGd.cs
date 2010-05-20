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
            DAL.Select q = new DAL.Select( "suc_shorturl" );
            q.setFrom("shorturlcache");
            q.addWhere( new DAL.WhereConds( "suc_fullurl", longUrl.ToString( ) ) );
            string cachelookup = DAL.Singleton( ).executeScalarSelect( q );

            if( cachelookup == "" )
            {

                HttpWebRequest wrq = (HttpWebRequest)WebRequest.Create( "http://is.gd/api.php?longurl=" + longUrl.ToString( ) );
                wrq.UserAgent = Configuration.Singleton( ).retrieveGlobalStringOption( "useragent" );
                HttpWebResponse wrs = (HttpWebResponse)wrq.GetResponse( );
                if( wrs.StatusCode == HttpStatusCode.OK )
                {
                    StreamReader sr = new StreamReader( wrs.GetResponseStream( ) );
                    string shorturl = sr.ReadLine( );
                    DAL.Singleton( ).Insert( "shorturlcache", "", longUrl.ToString( ), shorturl );
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
