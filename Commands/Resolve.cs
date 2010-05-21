using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace helpmebot6.Commands
{
    class Resolve : GenericCommand
    {
        protected override CommandResponseHandler execute( User source, string channel, string[ ] args )
        {
            IPAddress[ ] addresses = new IPAddress[ 0 ];
            try
            {
                addresses = Dns.GetHostEntry( args[ 0 ] ).AddressList;
            }
            catch( SocketException )
            {

            }
            if( addresses.Length != 0 )
            {
                string ipList = "";
                bool first = true;
                foreach( IPAddress item in addresses )
                {
                    if( !first )
                        ipList += ", ";
                    ipList += item.ToString( );
                    first = false;
                }
                string[ ] messageargs = { args[ 0 ], ipList };

                return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "resolve", messageargs ) );
            }
            else
            {
                string[ ] messageargs = { args[ 0 ] };
                return new CommandResponseHandler( Configuration.Singleton( ).GetMessage( "resolveFail", messageargs ) );

            }

        }
    }
}
