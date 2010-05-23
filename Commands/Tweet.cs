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
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            Twitter twit = new Twitter(
                    Configuration.Singleton( ).retrieveGlobalStringOption( "twitterUsername" ),
                    Configuration.Singleton( ).retrieveGlobalStringOption( "twitterPassword" )
                );

            twit.userAgent = Configuration.Singleton( ).retrieveGlobalStringOption( "useragent" );

            string status = string.Join( " ", args );

            HttpStatusCode wrsp = twit.statuses_update( status );
            if( wrsp == HttpStatusCode.OK )
                return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) );
            else
                throw new Exception( );
        }
    }
}
