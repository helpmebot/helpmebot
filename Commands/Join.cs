using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    /// <summary>
    /// Joins an IRC channel
    /// </summary>
    class Join : GenericCommand
    {
        public Join( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            string[ ] whereParams = { "channel_name = '" + args[ 0 ] + "'" };
            string count = DAL.Singleton( ).Select( "count(*)" , "channel" , null , whereParams , null , null , null , 1 , 0 );
            if( count == "1" )
            { // entry exists
                DAL.Singleton( ).ExecuteNonQuery( "update channel set channel_enabled = 1 where channel_name = \"" + args[ 0 ] + "\" limit 1;" );
                Helpmebot6.irc.IrcJoin( args[ 0 ] );
            }
            else
            {
                DAL.Singleton( ).ExecuteNonQuery( "insert into channel values ( null, \"" + args[ 0 ] + "\", \"\", 1,1);" );
                Helpmebot6.irc.IrcJoin( args[ 0 ] );
            }
            return null;
        } 
    }
}
