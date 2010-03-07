using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace helpmebot6.Commands
{
    class Decode : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            if( args[ 1 ].Length != 8 )
                return null;

            byte[ ] ip = new byte[ 4 ];
            ip[ 0 ] = Convert.ToByte( args[ 1 ].Substring( 0, 2 ), 16 );
            ip[ 1 ] = Convert.ToByte( args[ 1 ].Substring( 2, 2 ), 16 );
            ip[ 2 ] = Convert.ToByte( args[ 1 ].Substring( 4, 2 ), 16 );
            ip[ 3 ] = Convert.ToByte( args[ 1 ].Substring( 6, 2 ), 16 );

            IPAddress ipAddr = new IPAddress( ip );

            IPHostEntry iphe = Dns.GetHostEntry( ipAddr );
            
            string[] messageargs = { args[1], ipAddr.ToString(), iphe.HostName};

            return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "hexDecodeResult", messageargs ) );
        }
    }
}
