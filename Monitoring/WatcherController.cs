using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
namespace helpmebot6.Monitoring
{
    /// <summary>
    /// Controls instances of CategoryWatchers for the bot
    /// </summary>
    class WatcherController
    {
        ArrayList watchers;

        protected WatcherController()
        {
            
        }

        // woo singleton
        public static WatcherController Instance()
        {
            if (_instance == null)
                _instance = new WatcherController(); 
            return _instance;
        }
        private static WatcherController _instance;

        void AddWatcher(int site ,string category)
        {
            DAL.Singleton().ExecuteNonQuery("");
            // add to database

            // create watcher

            // add to watcher array
        }
        void DeleteWatcher()
        { 
            // remove from database

            // kill watcher

            // remove from watcher array
        }
      public  void ReloadWatchers()
        {
            // kill all watchers
            GlobalFunctions.Log("Stopping all category watchers...");
            foreach (CategoryWatcher item in watchers)
            {
                item.Stop();
            }

            // remove from array
            watchers.Clear();

            // load watchers from database
            MySql.Data.MySqlClient.MySqlDataReader dr = DAL.Singleton().ExecuteReaderQuery("");

        }

      public void forceUpdate( string key )
      {
          foreach( CategoryWatcher item in watchers )
          {
              if( item.ToString( ) == key )
              {
                  DotNetWikiBot.PageList list = item.doCategoryCheck( );
                  if( list.Count( ) > 0 )
                  { // items in cat
                      
                  }
                  else
                  { // nothing in cat, report anyway

                  }

                  break;
              }
          }
      }
    }
}
