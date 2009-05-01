/*
 * Helpmebot v6 by Simon Walker ( User:Stwalkerster , stwalkerster@googlemail.com )
 * 
 * This work is licenced under the Creative Commons 
 * Attribution-Share Alike 2.0 UK: England & Wales License.
 * To view a copy of this licence, visit 
 * http://creativecommons.org/licenses/by-sa/2.0/uk/ or send 
 * a letter to Creative Commons, 171 Second Street, 
 * Suite 300, San Francisco, California 94105, USA.
*/

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
       static CommandParser cmd;

       static string Trigger;

       public static string debugChannel;
       public static string mainChannel;

       static void Main( string[ ] args )
       {

           string server, username, password, schema;
           uint port = 0;
           server = username = password = schema = "";

           Configuration.readHmbotConfigFile( ".hmbot", ref server, ref username, ref password, ref port, ref schema );

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

           CommandParser.singleton = new CommandParser( );
           cmd = CommandParser.singleton;

           irc.ConnectionRegistrationSucceededEvent += new IAL.ConnectionRegistrationEventHandler( JoinChannels );
           irc.PrivmsgEvent += new IAL.PrivmsgEventHandler( RecievedMessage );
           irc.Connect( );
       }

        static void RecievedMessage( User source, string destination, string message )
        {
            // Bot AI
            string[] helloWords = { "hi", "hey", "heya", "morning", "afternoon", "evening", "hello" };
            if ( GlobalFunctions.isInArray( message.Split( ' ' )[ 0 ].ToLower( ), helloWords ) && message.Split( ' ' )[ 1 ].ToLower( ) == irc.IrcNickname.ToLower( ) )
            {
                cmd.CommandParser_CommandRecievedEvent( source, destination, "sayhi", null );
            }
            else
            {
                string[ ] splitMessage = message.Split( ' ' );
                if ( splitMessage[ 0 ] == Trigger + irc.IrcNickname )
                {
                    GlobalFunctions.popFromFront( ref splitMessage );
                }

                string command = GlobalFunctions.popFromFront( ref splitMessage ).Substring(1);
                if ( command.Substring( 0, 1 ) == Trigger )
                {
                    cmd.CommandParser_CommandRecievedEvent( source, destination, command, splitMessage );
                }
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
