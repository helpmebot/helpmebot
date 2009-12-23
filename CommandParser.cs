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
using System.Reflection;

namespace helpmebot6
{
    public class CommandParser
    {
        private bool _overrideBotSilence = false;

        public CommandParser( )
        {
        }

        public bool overrideBotSilence
        {
            get
            {
                return _overrideBotSilence;
            }
            set
            {
                _overrideBotSilence = value;
            }
        }

        public void handleCommand( User source , string destination , string command , string[ ] args )
        {
            Logger.Instance().addToLog( "Handling recieved message..." , Logger.LogTypes.GENERAL);

            // if on ignore list, ignore!
            if( source.AccessLevel == User.userRights.Ignored )
                return;

            // flip destination over if required
            if( destination == Helpmebot6.irc.IrcNickname )
                destination = source.Nickname;


            /*
             * check category codes
             */
            if( Monitoring.WatcherController.Instance( ).isValidKeyword( command ) )
            {
                int argsLength = GlobalFunctions.RealArrayLength( args );

                string[ ] newArgs = new string[ argsLength + 1 ];
                int newArrayPos = 1;
                for( int i = 0 ; i < args.Length ; i++ )
                {
                    if( !string.IsNullOrEmpty( args[ i ] ) )
                        newArgs[ newArrayPos ] = args[ i ];
                    newArrayPos++;
                }
                newArgs[ 0 ] = command;
                string directedTo = FindRedirection( destination , ref newArgs );
                CommandResponseHandler crh = new Commands.CategoryWatcher( ).run( source , destination, newArgs );
                HandleCommandResponseHandler( source , destination , directedTo , crh );
                return;

            }

            /* 
             * Check for a valid command
             * search for a class that can handle this command.
             */

            // Create a new object which holds the type of the command handler, if it exists.
            // if the command handler doesn't exist, then this won't be set to a value
            Type commandHandler = Type.GetType( "helpmebot6.Commands." + command.Substring( 0 , 1 ).ToUpper( ) + command.Substring( 1 ).ToLower() );
            // check the type exists
            if( commandHandler != null )
            {
                string directedTo = FindRedirection( destination , ref args );

                // create a new instance of the commandhandler.
                // cast to genericcommand (which holds all the required methods to run the command)
                // run the command.
                CommandResponseHandler response = ( (Commands.GenericCommand)Activator.CreateInstance( commandHandler ) ).run( source, destination , args );
                HandleCommandResponseHandler( source , destination , directedTo , response );
                return;
            }

            /*
             * Check for a learned word
             */
            {
                WordLearner.RemeberedWord rW = WordLearner.Remember( command );
                CommandResponseHandler crh = new CommandResponseHandler( );
                string wordResponse = rW.phrase;
                string directedTo = "";
                if( wordResponse != string.Empty )
                {
                    if( source.AccessLevel < User.userRights.Normal )
                    {
                        crh.respond( Configuration.Singleton( ).GetMessage( "accessDenied" ) , CommandResponseDestination.PRIVATE_MESSAGE );
                        string[ ] aDArgs = { source.ToString( ) , MethodBase.GetCurrentMethod( ).Name };
                        crh.respond( Configuration.Singleton( ).GetMessage( "accessDeniedDebug" , aDArgs ) , CommandResponseDestination.CHANNEL_DEBUG );

                    }
                    else
                    {
                        wordResponse = string.Format( wordResponse , args );
                        if( rW.action )
                        {
                            crh.respond( IAL.wrapCTCP( "ACTION" , wordResponse ) );
                        }
                        else
                        {
                            directedTo = FindRedirection( destination , ref args );
                            if( directedTo != string.Empty )
                            {
                                wordResponse = directedTo + ": " + wordResponse;
                            }
                            crh.respond( wordResponse );
                        }
                        HandleCommandResponseHandler( source , destination , directedTo , crh );
                    }
                    return;
                }
            }
        }



        private string FindRedirection( string destination , ref string[ ] args )
        {
            string directedTo = "";
            foreach( string arg in args )
            {
                if( arg.StartsWith( ">" ) )
                {
                    if( Helpmebot6.irc.isOnChannel( destination , arg.Substring( 1 ) ) != 0 )
                        directedTo = arg.Substring( 1 );

                    GlobalFunctions.removeItemFromArray( arg , ref args );
                }
            }
            return directedTo;
        }

        private void HandleCommandResponseHandler( User source , string destination , string directedTo , CommandResponseHandler response )
        {
            if( response != null )
            {
                foreach( CommandResponse item in response.getResponses( ) )
                {
                    string message = item.Message;

                    if( directedTo != string.Empty )
                    {
                        message = directedTo + ": " + message;
                    }

                    switch( item.Destination )
                    {
                        case CommandResponseDestination.DEFAULT:
                            if( this.overrideBotSilence || ! (Configuration.Singleton( ).retrieveLocalStringOption( "silence" , destination ) == "true"))
                            {
                                Helpmebot6.irc.IrcPrivmsg( destination , message );
                            }
                            break;
                        case CommandResponseDestination.CHANNEL_DEBUG:
                            Helpmebot6.irc.IrcPrivmsg( Helpmebot6.debugChannel , message );
                            break;
                        case CommandResponseDestination.PRIVATE_MESSAGE:
                            Helpmebot6.irc.IrcPrivmsg( source.Nickname , message );
                            break;
                    }

                }
            }
        }




    }
}
