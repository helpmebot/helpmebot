using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

namespace helpmebot6
{
    public class CategoryWatcher
    {

        Uri _apiUri;
        string _category;

        Thread watcherThread;

        int _sleepTime;


        public delegate void CategoryHasItemsEventDelegate( ArrayList items );
        public event CategoryHasItemsEventDelegate CategoryHasItemsEvent;

        

        public CategoryWatcher( string Category, Uri ApiUrl )
        {
            _apiUri = ApiUrl;
            _category = Category;

            watcherThread = new Thread( new ThreadStart( this.watcherThreadMethod ) );
            watcherThread.Start( );
           
        }

        private void watcherThreadMethod( )
        {
            try
            {
                while ( true )
                {
                    ArrayList categoryResults = this.doCategoryCheck( );
                    if ( categoryResults.Count > 0 )
                    {
                        CategoryHasItemsEvent( categoryResults );
                    }
                    Thread.Sleep( this.SleepTime );
                }
            }
            catch ( ThreadAbortException ex )
            {
                GlobalFunctions.ErrorLog( ex, System.Reflection.MethodInfo.GetCurrentMethod( ) );
            }
        }

        private ArrayList doCategoryCheck( )
        {
            return new ArrayList();
        }


        public int SleepTime
        {
            get
            {
                return _sleepTime;
            }
            set
            {
                _sleepTime = value;
            }
        }

    }
}
