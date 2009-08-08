using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
namespace helpmebot.Monitoring
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

        void AddWatcher()
        {
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
        void ReloadWatchers()
        {
            // kill all watchers

            // remove from array

            // load watchers from database
        }
    }
}
