/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using helpmebot6.Threading;
namespace helpmebot6
{
    public class Helpmebot6
    {
       public static IAL irc ;
       static DAL dbal;
       static Configuration config;
       static UdpListener.UDPListener udp;

       static string Trigger;

       public static string debugChannel;
       public static string mainChannel;

       static uint ircNetwork;

       public static readonly DateTime startupTime = DateTime.Now;
        
       static void Main( string[ ] args )
       {
           // startup arguments
           int configFileArg = GlobalFunctions.prefixIsInArray( "--configfile", args );
           string configFile = ".hmbot";
           if ( configFileArg!=-1 )
           {
               configFile = args[ configFileArg ].Substring( args[ configFileArg ].IndexOf( '=' ) );
           }

           if( GlobalFunctions.prefixIsInArray( "--logdal" , args ) != -1 )
               Logger.Instance( ).LogDAL = true;
           if( GlobalFunctions.prefixIsInArray( "--logdallock", args ) != -1 )
               Logger.Instance( ).LogDALLOCK = true;
           if( GlobalFunctions.prefixIsInArray( "--logirc" , args ) != -1 )
               Logger.Instance( ).LogIRC = true;


           InitialiseBot( configFile );
       }

       private static void InitialiseBot( string configFile )
       {
           string server, username, password, schema;
           uint port = 0;
           server = username = password = schema = "";

           Configuration.readHmbotConfigFile( configFile, ref server, ref username, ref password, ref port, ref schema );

           dbal = DAL.Singleton( server, port, username, password, schema );

           if( !dbal.Connect( ) )
           { // can't connect to database, DIE
               return;
           }

           config = Configuration.Singleton( );


           ircNetwork = config.retrieveGlobalUintOption( "ircNetwork" );


           Trigger = config.retrieveGlobalStringOption( "commandTrigger" );

           irc = new IAL( ircNetwork );

           Monitoring.PageWatcher.PageWatcherController.Instance( );

           SetupEvents( );

           NewYear.TimeMonitor.instance( );

           if( !irc.Connect( ) )
           { // if can't connect to irc, die
               return;
           }

           udp = new helpmebot6.UdpListener.UDPListener( 4357 );

           string[ ] twparms = { server, schema, irc.IrcServer };
           Twitter.tweet( Configuration.Singleton( ).GetMessage( "tweetStartup", twparms ) );
       }



       static void SetupEvents( )
       {
           irc.ConnectionRegistrationSucceededEvent += new IAL.ConnectionRegistrationEventHandler( JoinChannels );

           irc.JoinEvent += new IAL.JoinEventHandler( welcomeNewbieOnJoinEvent );

           irc.PrivmsgEvent += new IAL.PrivmsgEventHandler( ReceivedMessage );

           irc.InviteEvent += new IAL.InviteEventHandler( irc_InviteEvent );

           irc.ThreadFatalError += new EventHandler( irc_ThreadFatalError );

           Monitoring.PageWatcher.PageWatcherController.Instance( ).PageWatcherNotificationEvent += new helpmebot6.Monitoring.PageWatcher.PageWatcherController.PageWatcherNotificationEventDelegate( Helpmebot6_PageWatcherNotificationEvent );
       }

       static void irc_ThreadFatalError( object sender, EventArgs e )
       {
           Stop( );
       }

       static void Helpmebot6_PageWatcherNotificationEvent( helpmebot6.Monitoring.PageWatcher.PageWatcherController.RcPageChange rcItem )
       {
           string[ ] messageParams = { rcItem.title , rcItem.user , rcItem.comment , rcItem.diffUrl , rcItem.byteDiff , rcItem.flags };
           string message = Configuration.Singleton( ).GetMessage( "pageWatcherEventNotification" , messageParams );

           string[ ] selectCols = { "c.`channel_name`" };
           DAL.join[ ] jC = new DAL.join[ 2 ];
           jC[ 0 ].table = "`channel` c";
           jC[ 0 ].joinType = DAL.joinTypes.INNER;
           jC[ 0 ].joinConditions = "pwc.`pwc_channel` = c.`channel_id`";
           jC[ 1 ].table = "`watchedpages` wp";
           jC[ 1 ].joinType = DAL.joinTypes.INNER;
           jC[ 1 ].joinConditions = "wp.`pw_id` = pwc.`pwc_pagewatcher`";
           string[ ] wc = { "wp.`pw_title` = \"" + rcItem.title + "\";" };
           ArrayList channels = DAL.Singleton( ).Select( selectCols , "`pagewatcherchannels` pwc" , jC , wc , null , null , null , 0 , 0 );

           foreach( object[] item in channels )
           {
               irc.IrcPrivmsg( (string)item[ 0 ] , message );
           }
       }

       static void irc_InviteEvent( User source , string nickname , string channel )
       {
           string[ ] args = { channel };
           new Commands.Join( ).run( source ,channel, args);
       }

       static void welcomeNewbieOnJoinEvent( User source , string channel )
       {
           Monitoring.NewbieWelcomer.Instance( ).execute( source, channel );
       }

       static void ReceivedMessage( User source , string destination , string message )
       {
           CommandParser cmd = new CommandParser( );
           try
           {
               bool overrideSilence = cmd.overrideBotSilence;
               if( isRecognisedMessage( ref message , ref overrideSilence ) )
               {
                   cmd.overrideBotSilence = overrideSilence;
                   string[ ] messageWords = message.Split( ' ' );
                   string command = messageWords[ 0 ];
                   string[ ] commandArgs = string.Join( " " , messageWords , 1 , messageWords.Length - 1 ).Split( ' ' );

                   cmd.handleCommand( source , destination , command , commandArgs );


               }
               string aiResponse = AI.Intelligence.Singleton( ).Respond( message );
               if( aiResponse != string.Empty )
               {
                   string[ ] aiParameters = { source.Nickname };
                   irc.IrcPrivmsg( destination, config.GetMessage( aiResponse, aiParameters ) );
               }
           }
           catch( Exception ex )
           {
               GlobalFunctions.ErrorLog( ex  );
           }


       }

       static void JoinChannels( )
        {
            debugChannel = config.retrieveGlobalStringOption( "channelDebug" );
            irc.IrcJoin( debugChannel );

            MySql.Data.MySqlClient.MySqlDataReader dr = dbal.ExecuteReaderQuery( "SELECT `channel_name` FROM `channel` WHERE `channel_enabled` = 1 AND `channel_network` = '"+ircNetwork.ToString()+"';" );
            if( dr != null )
            {
                while( dr.Read( ) )
                {
                    object[ ] channel = new object[ 1 ];
                    dr.GetValues( channel );
                    irc.IrcJoin( channel[ 0 ].ToString( ) );
                }
                dr.Close( );
            }
        }

        /// <summary>
        /// Tests against recognised message formats
        /// </summary>
        /// <param name="message">the message recieved</param>
        /// <param name="overrideSilence">ref: whether this message format overrides any imposed silence</param>
        /// <returns>true if the message is in a recognised format</returns>
        /// <remarks>Allowed formats:
        /// !command
        /// !helpmebot command
        /// Helpmebot: command
        /// Helpmebot command
        /// Helpmebot, command
        /// Helpmebot> command
        /// </remarks>
       static bool isRecognisedMessage( ref string message , ref bool overrideSilence )
       {
           string[ ] words = message.Split( ' ' );

           if( words[ 0 ].StartsWith( Trigger ) )
           {

               /// !

               if( message.Length == Trigger.Length )
                   return false;

               /// !command
               /// !helpmebot command


               if( words[ 0 ].ToLower() == ( Trigger + irc.IrcNickname.ToLower()) )
               {
                   overrideSilence = true;
                   message = string.Join( " " , words , 1 , words.Length - 1 );
                   return true;
               }
               else
               {
                   message = message.Substring( 1 );
                   overrideSilence = false;
                   return true;
               }
           }
           else
           {
               if( words[ 0 ].ToLower() == irc.IrcNickname.ToLower() )/// Helpmebot command
               {
                   message = string.Join( " " , words , 1 , words.Length - 1 );
                   overrideSilence = true;
                   return true;
               }
               else if( words[ 0 ].ToLower() == ( irc.IrcNickname.ToLower() + ":" ) ) /// Helpmebot: command
               {
                   message = string.Join( " " , words , 1 , words.Length - 1 );
                   overrideSilence = true;
                   return true;
               }
               else if( words[ 0 ].ToLower() == ( irc.IrcNickname.ToLower() + ">" ) ) /// Helpmebot> command
               {
                   message = string.Join( " " , words , 1 , words.Length - 1 );
                   overrideSilence = true;
                   return true;
               }
               else if(words[ 0 ].ToLower() == (irc.IrcNickname.ToLower() + ",")) /// Helpmebot, command
               {
                   message = string.Join( " " , words , 1 , words.Length - 1 );
                   overrideSilence = true;
                   return true;
               }

           }
           return false;
       }

       static public void Stop( )
       {
           ThreadList.instance( ).stop( );
       }
    }
}
