using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace helpmebot6.Threading
{
    class ThreadList
    {
        private static ThreadList _instance;
        public static ThreadList instance( )
        {
            if( _instance == null )
                _instance = new ThreadList( );

            return _instance;
        }
        protected ThreadList( )
        {
            threadedObjects = new ArrayList( );
        }

        ArrayList threadedObjects;

        public void register( IThreadedSystem sender )
        {
            threadedObjects.Add( sender );
        }

        public void stop( )
        {
            System.Threading.Thread shutdownControllerThread
                    = new System.Threading.Thread( new System.Threading.ThreadStart( shutdown_method ) );

            shutdownControllerThread.Start();
        }

        private void shutdown_method()
        {
            foreach( object obj in threadedObjects )
            {
                try
                {
                    Logger.Instance( ).addToLog( "Attempting to shut down threaded system: " + obj.GetType( ), Logger.LogTypes.GENERAL );
                    ( (IThreadedSystem)obj ).Stop( );
                }
                catch( NotImplementedException ex )
                {
                    GlobalFunctions.ErrorLog( ex );
                }
            }

            Logger.Instance( ).addToLog( "All threaded systems have been shut down.", Logger.LogTypes.GENERAL );
        }

        public string[ ] getAllThreadStatus( )
        {
            ArrayList responses = new ArrayList( );
            foreach( IThreadedSystem item in threadedObjects )
            {
                string status = item.GetType( ).ToString( ) + ": ";
                try
                {
                    foreach( string i in item.getThreadStatus( ) )
                    {
                        responses.Add( status + i );
                    }
                }
                catch( NotImplementedException )
                {
                    status += "Not available.";
                    responses.Add( status );
                }

            }

            string[ ] responseArray = new string[ responses.Count ];

            responses.CopyTo( responseArray );

            return responseArray;
        }
    }
}
