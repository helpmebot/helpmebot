using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace helpmebot6.Commands
{
    class Tweet : GenericCommand 
    {

        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create( "http://api.twitter.com/1/statuses/update.xml" );
            wr.Credentials = new NetworkCredential(
                    Configuration.Singleton( ).retrieveGlobalStringOption( "twitterUsername" ),
                    Configuration.Singleton( ).retrieveGlobalStringOption( "twitterPassword" )
                );
            wr.Method = "POST";
            wr.UserAgent = Configuration.Singleton( ).retrieveGlobalStringOption( "useragent" );
            string status = string.Join( " ", args );
            status = (status.Length > 140 ? status.Substring( 0, 140 ) : status);
            string postData = "status=" + status;

            wr.ContentType = "application/x-www-form-urlencoded";
            wr.ContentLength = postData.Length;
            System.Net.ServicePointManager.Expect100Continue = false;
            using( Stream writeStream = wr.GetRequestStream( ) )
            {
                UTF8Encoding encoding = new UTF8Encoding( );
                byte[ ] bytes = encoding.GetBytes( postData );
                writeStream.Write( bytes, 0, bytes.Length );
            }

            HttpWebResponse wrsp = (HttpWebResponse)wr.GetResponse( );
            if( wrsp.StatusCode == HttpStatusCode.OK )
                return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) );
            else
                throw new Exception( );
        }
    }
}
