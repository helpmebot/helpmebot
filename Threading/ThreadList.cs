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
            foreach( object obj in threadedObjects )
            {
                try
                {
                    ( (IThreadedSystem)obj ).Stop( );
                }
                catch( NotImplementedException ex )
                {
                    GlobalFunctions.ErrorLog( ex );
                }
            }
        }

        public string[ ] getAllThreadStatus( )
        {
            ArrayList responses = new ArrayList( );
            foreach( IThreadedSystem item in threadedObjects )
            {
                responses.Add( item.getThreadStatus( ) );
            }

            string[ ] responseArray = new string[ responses.Count ];

            responses.CopyTo( responseArray );

            return responseArray;
        }
    }
}
