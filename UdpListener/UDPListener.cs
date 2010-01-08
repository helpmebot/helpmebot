using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace helpmebot6.UdpListener
{
    public class UDPListener
    {
        public UDPListener( int port )
        {
            key = Configuration.Singleton( ).retrieveGlobalStringOption( "udpKey" );
            udpClient = new UdpClient( port );
            listenerThread = new Thread( new ThreadStart( threadMethod ) );
            listenerThread.Start( );
        }

        
        Thread listenerThread;
        UdpClient udpClient;
        string key;

        private void threadMethod( )
        {
            try
            {
                while( true )
                {
                    IPEndPoint ipep = new IPEndPoint( IPAddress.Any, 0 );
                     byte[ ] datagram = udpClient.Receive( ref ipep );

                    BinaryFormatter bf = new BinaryFormatter( );
                    UdpMessage recievedMessage = (UdpMessage)bf.Deserialize( new MemoryStream( datagram ) );

                    byte[ ] bm = ASCIIEncoding.ASCII.GetBytes( recievedMessage.Message + key );
                    byte[ ] bh = MD5.Create( ).ComputeHash( bm );
                    string hash = ASCIIEncoding.ASCII.GetString( bh );
                    if( hash == recievedMessage.Hash )
                    {
                        Helpmebot6.irc.SendRawLine( "PRIVMSG " + recievedMessage.Message );
                    }
                }
            }
            catch( ThreadAbortException ex )
            {
                GlobalFunctions.ErrorLog( ex );
            }
        }
    }
}
