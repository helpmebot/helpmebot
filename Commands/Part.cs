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
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            Helpmebot6.irc.IrcPart( channel , source.ToString( ) );
            Dictionary<string, string> vals = new Dictionary<string, string>( );
            vals.Add( "channel_enabled", "0" );
            DAL.Singleton( ).Update( "channel", vals, 0, new DAL.WhereConds( "channel_name", channel ), new DAL.WhereConds( "channel_network", source.Network.ToString( ) ) );
            return null;
        }
    }
}