using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Part : GenericCommand
    {
        public Part( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Helpmebot6.irc.IrcPart( channel , source.ToString( ) );
            DAL.Singleton( ).ExecuteNonQuery( "UPDATE channel SET channel_enabled = 0 WHERE channel_name = \"" + channel + "\";" );
            return null;
        }
    }
}