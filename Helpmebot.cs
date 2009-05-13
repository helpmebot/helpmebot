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

           DAL.singleton = new DAL( server, port, username, password, schema );
           dbal = DAL.singleton;
           dbal.Connect( );

           Configuration.singleton = new Configuration( );
           config = Configuration.singleton;

           string Nickname, Username, Realname, Password, Server;
           uint Port;
           bool Wallops, Invisible;
           Nickname = config.retrieveStringOption( "ircNickname" );
           Username = config.retrieveStringOption( "ircUsername" );
           Realname = config.retrieveStringOption( "ircRealname" );
           Password = config.retrieveStringOption( "ircPassword" );
           Port = config.retrieveUintOption( "ircServerPort" );
           Server = config.retrieveStringOption( "ircServerHost" );
           Wallops = false;
           Invisible = false;

           Trigger = config.retrieveStringOption( "commandTrigger" );

           IAL.singleton = new IAL( Nickname, Username, Realname, Password, Server, Port, Wallops, Invisible );
           irc = IAL.singleton;

//           CommandParser.singleton = new CommandParser( );
//           cmd = CommandParser.singleton;

           irc.ConnectionRegistrationSucceededEvent += new IAL.ConnectionRegistrationEventHandler( JoinChannels );
           irc.PrivmsgEvent += new IAL.PrivmsgEventHandler( RecievedMessage );
           irc.Connect( );
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
                    if ( message.Substring( 0, 1 ) == Trigger )
                    {
                        message = message.Substring( 1 );

                        if ( message.Split( ' ' )[ 0 ] == irc.IrcNickname.ToLower( ) )
                        {

                        }
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
            debugChannel = config.retrieveStringOption( "channelDebug" );
            mainChannel = config.retrieveStringOption( "channelMain" );

            irc.IrcJoin( debugChannel );
            irc.IrcJoin( mainChannel );
        }

        

    }
}
