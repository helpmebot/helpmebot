using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using helpmebot6.Threading;

namespace helpmebot6.Monitoring
{
    class MonitorService : IThreadedSystem
    {
        TcpListener service;

        bool alive;

        Thread monitorthread;

        readonly string message;

        public MonitorService( int port, string message )
        {
            monitorthread = new Thread( new ThreadStart( threadMethod ) );

            this.message = message;

            service = new TcpListener( System.Net.IPAddress.Any, port );
            RegisterInstance( );
            monitorthread.Start( );
        }

        void threadMethod( )
        {
            try
            {
                alive = true;
                service.Start( );

                while( service.Pending( ) || alive )
                {
                    if( !service.Pending( ) )
                    {
                        Thread.Sleep( 10 );
                        continue;
                    }

                    TcpClient client = service.AcceptTcpClient( );

                    StreamWriter sw = new StreamWriter( client.GetStream( ) );

                    sw.WriteLine( message );
                    sw.Flush( );
                    client.Close( );
                }
            }
            catch( ThreadAbortException )
            {
                this.ThreadFatalError( this, new EventArgs( ) );
            }
        }

        public void Stop( )
        {
            service.Stop( );
            alive = false;

        }

        #region IThreadedSystem Members

        public void RegisterInstance( )
        {
            ThreadList.instance( ).register( this );
        }

        public string[ ] getThreadStatus( )
        {
            string[ ] status = { "NagiosMonitor thread: " + monitorthread.ThreadState.ToString( ) };
            return status;
        }

        public event EventHandler ThreadFatalError;

        #endregion
    }
}