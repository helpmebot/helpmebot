using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace helpmebot6
{
    enum CommandResponseDestination
    {
        DEFAULT,
        CHANNEL_DEBUG,
        PRIVATE_MESSAGE
    }

    struct CommandResponse
    {
        public CommandResponseDestination Destination;
        public string Message;
    }

    class CommandResponseHandler
    {
        ArrayList responses;
        string debugIrcChannel;

        public CommandResponseHandler( )
        {
            responses = new ArrayList( );
        }
        public CommandResponseHandler( string message )
        {
            responses = new ArrayList( );
            debugIrcChannel = helpmebot6.Configuration.Singleton( ).retrieveGlobalStringOption( "debugIrcChannel" );
            respond( message );
        }
        public CommandResponseHandler( string message , CommandResponseDestination destination )
        {
            responses = new ArrayList( );
            debugIrcChannel = helpmebot6.Configuration.Singleton( ).retrieveGlobalStringOption( "debugIrcChannel" );
            respond( message , destination );
        }
        public void respond( string message  )
        {
            CommandResponse cr;
            cr.Destination = CommandResponseDestination.DEFAULT;
            cr.Message = message;

            responses.Add( cr );
        }
        public void respond( string message, CommandResponseDestination destination )
        {
            CommandResponse cr;
            cr.Destination = destination;
            cr.Message = message;
                
            responses.Add( cr );
        }

        public ArrayList getResponses()
        {
            return responses;
        }
    }
}
