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

        public CommandResponseHandler( )
        {
            responses = new ArrayList( );
        }
        public CommandResponseHandler( string message )
        {
            responses = new ArrayList( );
            respond( message );
        }
        public CommandResponseHandler( string message , CommandResponseDestination destination )
        {
            responses = new ArrayList( );
            respond( message , destination );
        }
        public void respond( string message  )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            CommandResponse cr;
            cr.Destination = CommandResponseDestination.DEFAULT;
            cr.Message = message;

            responses.Add( cr );
        }
        public void respond( string message, CommandResponseDestination destination )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            CommandResponse cr;
            cr.Destination = destination;
            cr.Message = message;
                
            responses.Add( cr );
        }

        public void append( CommandResponseHandler moreResponses )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            foreach( object item in moreResponses.getResponses() )
            {
                responses.Add( item );
            }
        }

        public ArrayList getResponses()
        {
            return responses;
        }
    }
}
