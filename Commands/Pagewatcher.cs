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

            DAL.Select q = new DAL.Select( "COUNT(*)" );
            q.setFrom( "watchedpages" );
            q.addWhere( new DAL.WhereConds( "pw_title", page ) );

            // look to see if watchedpage exists
            if( DAL.Singleton( ).executeScalarSelect( q ) == "0" )
            {//    no: addOrder it
                DAL.Singleton( ).Insert( "watchedpages", "", page );
            }
            
            // get id of watchedpage
            q = new DAL.Select( "pw_id" );
            q.setFrom( "watchedpages" );
            q.addWhere( new DAL.WhereConds( "pw_title", page ) );

            string watchedPageId = DAL.Singleton( ).executeScalarSelect( q );
            
            // get id of channel
            string channelId = Configuration.Singleton( ).getChannelId( channel );
            
            // addOrder to pagewatcherchannels
            DAL.Singleton( ).Insert( "pagewatcherchannels", channelId, watchedPageId );

            Monitoring.PageWatcher.PageWatcherController.Instance( ).LoadAllWatchedPages( );

            return null;
        }

        private CommandResponseHandler removePageWatcher( string page , string channel )
        {
            // get id of watchedpage
           DAL.Select q = new DAL.Select( "pw_id" );
            q.setFrom( "watchedpages" );
            q.addWhere( new DAL.WhereConds( "pw_title", page ) );

            string watchedPageId = DAL.Singleton( ).executeScalarSelect( q );

            // get id of channel
            string channelId = Configuration.Singleton( ).getChannelId( channel );

            // remove from pagewatcherchannels
            DAL.Singleton( ).Delete( "pagewatcherchannels", 0, new DAL.WhereConds( "pwc_channel", channelId ), new DAL.WhereConds( "pwc_pagewatcher", watchedPageId ) );

            Monitoring.PageWatcher.PageWatcherController.Instance( ).LoadAllWatchedPages( );

            return null;
        }
        
    }
}
