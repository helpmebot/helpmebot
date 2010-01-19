using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Welcomer : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            switch( args[0].ToLower() )
            {
                case "enable":
                    if( Configuration.Singleton( ).retrieveLocalStringOption( "welcomeNewbie", channel ) == "true" )
                    {
                        return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "no-change" ) );
                    }
                    else
                    {
                        Configuration.Singleton( ).setLocalOption( "welcomeNewbie", channel, "true" );
                        return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) );
                    }
                case "disable":
                    if( Configuration.Singleton( ).retrieveLocalStringOption( "welcomeNewbie", channel ) == "false" )
                    {
                        return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "no-change" ) );
                    }
                    else
                    {
                        Configuration.Singleton( ).setLocalOption( "welcomeNewbie", channel, "false" );
                        return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) );
                    }
                case "global":
                    Configuration.Singleton( ).deleteLocalOption( "welcomeNewbie", channel );
                    return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "defaultSetting" ) );
                case "add":
                    helpmebot6.Monitoring.NewbieWelcomer.Instance( ).addHost( args[ 1 ] );
                    return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) );
                case "del":
                    helpmebot6.Monitoring.NewbieWelcomer.Instance( ).delHost( args[ 1 ] );
                    return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "done" ) );
                case "list":
                    CommandResponseHandler crh = new CommandResponseHandler( );
                    string[ ] list = helpmebot6.Monitoring.NewbieWelcomer.Instance( ).getHosts( );
                    foreach( string item in list )
                    {
                        crh.respond( item );
                    }
                    return crh;
            }
            return new CommandResponseHandler( );
        }
    }
}
