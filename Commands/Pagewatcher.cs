using System;
using System.Collections.Generic;
using System.Text;

namespace helpmebot6.Commands
{
    class Pagewatcher : GenericCommand
    {
        public Pagewatcher( )
        {

        }

        protected override CommandResponseHandler execute( User source , string channel , string[ ] args )
        {
            if( args.Length > 1 )
            {
                switch( GlobalFunctions.popFromFront( ref args ).ToLower() )
                {
                    case "add":
                        return addPageWatcher( string.Join( " ", args ), channel );
                    case "del":
                        return removePageWatcher( string.Join( " ", args ), channel );
                    case "list":
                        CommandResponseHandler crh = new CommandResponseHandler( );
                        foreach( string item in Monitoring.PageWatcher.PageWatcherController.Instance().getWatchedPages() )
                        {
                            crh.respond(item);
                        }
                        return crh;
                }
            }
            return new CommandResponseHandler( );
        }

        private CommandResponseHandler addPageWatcher(string page, string channel)
        {
            string[] wc = {"w.`pw_title` = '" + page + "'"};

            // look to see if watchedpage exists
            if( DAL.Singleton( ).Select( "COUNT(*)" , "helpmebot_v6.watchedpages w" , null , wc , null , null , null , 0 , 0 ) == "0" )
            {//    no: add it
                DAL.Singleton( ).ExecuteNonQuery( "INSERT INTO watchedpages VALUES ( null, \"" + page + "\");" );
            }
            
            // get id of watchedpage
            string watchedPageId = DAL.Singleton( ).Select( "w.`pw_id`" , "helpmebot_v6.watchedpages w" , null , wc , null , null , null , 1 , 0 );
            
            // get id of channel
            string channelId = Configuration.Singleton( ).getChannelId( channel );
            
            // add to pagewatcherchannels
            DAL.Singleton( ).ExecuteNonQuery( "INSERT INTO pagewatcherchannels VALUES ( " + channelId + ", " + watchedPageId + ");" );

            return null;
        }

        private CommandResponseHandler removePageWatcher( string page , string channel )
        {
            string[ ] wc = { "w.`pw_title` = '" + page + "'" };

            // get id of watchedpage
            string watchedPageId = DAL.Singleton( ).Select( "w.`pw_id`" , "helpmebot_v6.watchedpages w" , null , wc , null , null , null , 1 , 0 );

            // get id of channel
            string channelId = Configuration.Singleton( ).getChannelId( channel );

            // remove from pagewatcherchannels
            DAL.Singleton( ).ExecuteNonQuery( "DELETE FROM pagewatcherchannels WHERE pwc_channel = " + channelId + " AND pwc_pagewatcher = " + watchedPageId + ";" );

            return null;
        }
        
    }
}
