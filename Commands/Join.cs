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

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            DAL.Select q = new DAL.Select(  "count(*)");
            q.addWhere( new DAL.WhereConds( "channel_name", args[ 0 ] ) );
            q.addWhere( new DAL.WhereConds( "channel_network", source.Network.ToString( ) ) );
            q.setFrom("channel");

            string count = DAL.Singleton( ).executeScalarSelect( q );


            if( count == "1" )
            { // entry exists

                Dictionary<string, string> vals = new Dictionary<string, string>( );
                vals.Add( "channel_enabled", "1" );
                DAL.Singleton( ).Update( "channel", vals, 1, new DAL.WhereConds( "channel_name", args[ 0 ] ) );

                Helpmebot6.irc.IrcJoin( args[ 0 ] );
            }
            else
            {
                DAL.Singleton( ).Insert( "channel", "", args[ 0 ], "", "1", source.Network.ToString( ) );
                Helpmebot6.irc.IrcJoin( args[ 0 ] );
            }
            return null;
        } 
    }
}
