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

namespace helpmebot6
{
    public class Helpmebot6
    {
       public static IAL irc ;
       static DAL dbal;
       static Configuration config;
//       static CommandParser cmd;

       static string Trigger;

       public static string debugChannel;
       public static string mainChannel;

       static void Main( string[ ] args )
       {
           // startup arguments
           int configFileArg = GlobalFunctions.prefixIsInArray( "--configfile", args );
           string configFile = ".hmbot";
           if ( configFileArg!=-1 )
           {
               configFile = args[ configFileArg ].Substring( args[ configFileArg ].IndexOf( '=' ) );
           }

           InitialiseBot( configFile );
       }

       private static void InitialiseBot( string configFile )
       {
           string server, username, password, schema;
           uint port = 0;
           server = username = password = schema = "";

           Configuration.readHmbotConfigFile( configFile, ref server, ref username, ref password, ref port, ref schema );

           dbal= DAL.Singleton( server, port, username, password, schema );

           dbal.Connect( );

           config = Configuration.Singleton();


           uint ircNetwork = config.retrieveGlobalUintOption( "ircNetwork" );


           Trigger = config.retrieveGlobalStringOption( "commandTrigger" );

           irc = new IAL( ircNetwork );

           SetupEvents( );

           irc.Connect( );

           // initialise watcher controller
           Monitoring.WatcherController.Instance( );
       }

       static void SetupEvents( )
       {
           irc.ConnectionRegistrationSucceededEvent += new IAL.ConnectionRegistrationEventHandler( JoinChannels );

           irc.JoinEvent += new IAL.JoinEventHandler( welcomeNewbieOnJoinEvent );

           irc.PrivmsgEvent += new IAL.PrivmsgEventHandler( ReceivedMessage );

           irc.InviteEvent += new IAL.InviteEventHandler( irc_InviteEvent );
       }

       static void irc_InviteEvent( User source , string nickname , string channel )
       {
           string[ ] args = { channel };
           new Commands.Join( ).run( source , args);
       }


       static void welcomeNewbieOnJoinEvent( User source , string channel )
       {
           if( Configuration.Singleton( ).retrieveLocalStringOption( "welcomeNewbie" , channel ) == "true" )
           {
               string[ ] cmdArgs = { source.Nickname , channel };
               Helpmebot6.irc.IrcPrivmsg( channel , Configuration.Singleton( ).GetMessage( "welcomeMessage" , cmdArgs ) );
           }
       }

       static void ReceivedMessage( User source , string destination , string message )
       {
           CommandParser cmd = new CommandParser( );
           try
           {
               bool overrideSilence = cmd.overrideBotSilence;
               if( isRecognisedMessage( ref message , ref overrideSilence ) )
               {
                   string[ ] messageWords = message.Split( ' ' );
                   string command = messageWords[ 0 ];
                   string[ ] commandArgs = string.Join( " " , messageWords , 1 , messageWords.Length - 1 ).Split( ' ' );

                   cmd.handleCommand( source , destination , command , commandArgs );


               }
               string aiResponse = AI.Intelligence.Singleton( ).Respond( message );
               if( aiResponse != string.Empty )
               {

                   irc.IrcPrivmsg( destination , config.GetMessage( aiResponse , source.Nickname ) );
               }
           }
           catch( Exception ex )
           {
               GlobalFunctions.ErrorLog( ex , System.Reflection.MethodInfo.GetCurrentMethod( ) );
           }


       }

       static void JoinChannels( )
        {
            debugChannel = config.retrieveGlobalStringOption( "channelDebug" );
            irc.IrcJoin( debugChannel );
            
            MySql.Data.MySqlClient.MySqlDataReader dr = dbal.ExecuteReaderQuery("SELECT `channel_name` FROM `channel` WHERE `channel_enabled` = 1;");
            while (dr.Read())
            {
                object[] channel = new object[1];
                dr.GetValues(channel);
                irc.IrcJoin(channel[0].ToString());
            }
            dr.Close();
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
               /// !command
               /// !helpmebot command

               if( words[ 0 ] == ( Trigger + irc.IrcNickname ) )
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
               if( words[ 0 ] == irc.IrcNickname )/// Helpmebot command
               {
                   message = string.Join( " " , words , 1 , words.Length - 1 );
                   overrideSilence = true;
                   return true;
               }
               else if( words[ 0 ] == ( irc.IrcNickname + ":" ) ) /// Helpmebot: command
               {
                   message = string.Join( " " , words , 1 , words.Length - 1 );
                   overrideSilence = true;
                   return true;
               }
               else if( words[ 0 ] == ( irc.IrcNickname + ">" ) ) /// Helpmebot> command
               {
                   message = string.Join( " " , words , 1 , words.Length - 1 );
                   overrideSilence = true;
                   return true;
               }
               else if(words[ 0 ] == (irc.IrcNickname + ",")) /// Helpmebot, command
               {
                   message = string.Join( " " , words , 1 , words.Length - 1 );
                   overrideSilence = true;
                   return true;
               }

           }
           return false;
       }
        

    }
}
