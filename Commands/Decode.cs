using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace helpmebot6.Commands
{
    class Decode : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            Logger.Instance( ).addToLog( "Method:" + System.Reflection.MethodInfo.GetCurrentMethod( ).DeclaringType.Name + System.Reflection.MethodInfo.GetCurrentMethod( ).Name, Logger.LogTypes.DNWB );

            if( args[ 0 ].Length != 8 )
                return null;

            byte[ ] ip = new byte[ 4 ];
            ip[ 0 ] = Convert.ToByte( args[ 0 ].Substring( 0, 2 ), 16 );
            ip[ 1 ] = Convert.ToByte( args[ 0 ].Substring( 2, 2 ), 16 );
            ip[ 2 ] = Convert.ToByte( args[ 0 ].Substring( 4, 2 ), 16 );
            ip[ 3 ] = Convert.ToByte( args[ 0 ].Substring( 6, 2 ), 16 );

            IPAddress ipAddr = new IPAddress( ip );

            string hostname = "";
            try
            {
                hostname = Dns.GetHostEntry( ipAddr ).HostName;
            }
            catch( SocketException )
            {

            }
            if( hostname != string.Empty )
            {
                string[ ] messageargs = { args[ 0 ], ipAddr.ToString( ), hostname };
                return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "hexDecodeResult", messageargs ) );

            }
            else
            {
                string[ ] messageargs = { args[ 0 ], ipAddr.ToString( ) };
                return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "hexDecodeResultNoResolve", messageargs ) );

            }
        }
    }
}
