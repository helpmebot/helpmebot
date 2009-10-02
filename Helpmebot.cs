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
       static IAL irc ;
       static DAL dbal;
       static Configuration config;
//       static CommandParser cmd;

       static string Trigger;

       public static string debugChannel;
       public static string mainChannel;

       static void Main( string[ ] args )
       {
           bool initialiseBotWhenDone = true;

           // startup arguments
           int configFileArg = GlobalFunctions.prefixIsInArray( "--configfile", args );
           string configFile = ".hmbot";
           if ( configFileArg!=-1 )
           {
               configFile = args[ configFileArg ].Substring( args[ configFileArg ].IndexOf( '=' ) );
           }


           if ( initialiseBotWhenDone )
           {
               InitialiseBot( configFile );
           }
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

           string Nickname, Username, Realname, Password, Server;
           uint Port;
           bool Wallops, Invisible;
           Nickname = config.retrieveGlobalStringOption( "ircNickname" );
           Username = config.retrieveGlobalStringOption( "ircUsername" );
           Realname = config.retrieveGlobalStringOption( "ircRealname" );
           Password = config.retrieveGlobalStringOption( "ircPassword" );
           Port = config.retrieveGlobalUintOption( "ircServerPort" );
           Server = config.retrieveGlobalStringOption( "ircServerHost" );
           Wallops = false;
           Invisible = false;

           Trigger = config.retrieveGlobalStringOption( "commandTrigger" );

           IAL.singleton = new IAL( Nickname, Username, Realname, Password, Server, Port, Wallops, Invisible );
           irc = IAL.singleton;

//           CommandParser.singleton = new CommandParser( );
//           cmd = CommandParser.singleton;

           SetupEvents( );

           irc.Connect( );
       }

       static void SetupEvents( )
       {
           irc.ConnectionRegistrationSucceededEvent += new IAL.ConnectionRegistrationEventHandler( JoinChannels );

           irc.JoinEvent += new IAL.JoinEventHandler( welcomeNewbieOnJoinEvent );

           irc.PrivmsgEvent += new IAL.PrivmsgEventHandler( RecievedMessage );

           irc.InviteEvent += new IAL.InviteEventHandler( irc_InviteEvent );
       }

       static void irc_InviteEvent( User source , string nickname , string channel )
       {
           string[ ] args = { channel };
           new Commands.Join( ).run( source , null , args);
       }


       static void welcomeNewbieOnJoinEvent( User source , string channel )
       {
           if( Configuration.Singleton( ).retrieveLocalStringOption( "welcomeNewbie" , channel ) == "true" )
           {
               string[ ] cmdArgs = { source.Nickname , channel };
               IAL.singleton.IrcPrivmsg( channel , Configuration.Singleton( ).GetMessage( "welcomeMessage" , cmdArgs ) );
           }
       }

        static void RecievedMessage( User source, string destination, string message )
        {
            CommandParser cmd = new CommandParser( );
            try
            {
                // Bot AI
                string[ ] helloWords = { "hi", "hey", "heya", "morning", "afternoon", "evening", "hello" };
                if ( GlobalFunctions.isInArray( message.Split( ' ' )[ 0 ].ToLower( ), helloWords ) != -1 && message.Split( ' ' )[ 1 ].ToLower( ) == irc.IrcNickname.ToLower( ) )
                {
                    cmd.CommandParser_CommandRecievedEvent( source, destination, "sayhi", null );
                }
                else
                {
                    if( message.Substring( 0 , 1 ) == Trigger )
                    {
                        message = message.Substring( 1 );

                        string[] messageWords = message.Split( ' ' );

                        if( messageWords[ 0 ] == irc.IrcNickname.ToLower( ) )
                        {
                            cmd.overrideBotSilence = true;
                            messageWords = ( string.Join( " " , messageWords , 1 , messageWords.Length - 1 ) ).Split( ' ' );
                        }

                        string command = messageWords[ 0 ];
                        string[] commandArgs = ( string.Join( " " , messageWords , 1 , messageWords.Length - 1 ) ).Split( ' ' );


                        cmd.CommandParser_CommandRecievedEvent( source , destination , command , commandArgs );
                    }
                    else
                    {
                        // bot name prefixed eg:
                        // Helpmebot: helpme
                        // instead of:
                        // !helpmebot helpme
                        // or:
                        // !helpme


                    }
                }
            }
            catch ( Exception ex )
            {
                GlobalFunctions.ErrorLog( ex, System.Reflection.MethodInfo.GetCurrentMethod( ) );
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

        

    }
}
