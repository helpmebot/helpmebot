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
        //public static CommandParser singleton;

        private bool _overrideBotSilence = false;

        Configuration config = Configuration.Singleton();

        public delegate void CommandEventHandler( User source, string destination , string command, string[ ] args );
        public event CommandEventHandler CommandRecievedEvent;

        public CommandParser( )
        {
            CommandRecievedEvent += new CommandEventHandler( CommandParser_CommandRecievedEvent );
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

        public void CommandParser_CommandRecievedEvent( User source , string destination , string command , string[ ] args )
        {
            // if on ignore list, ignore!
            if( source.AccessLevel == User.userRights.Ignored )
                return;


            // Check for a learned word
            WordLearner.RemeberedWord rW = WordLearner.Remember( command );
            string wordResponse = rW.phrase;
            if( wordResponse != string.Empty )
            {
                if( source.AccessLevel < User.userRights.Normal )
                {
                    IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "accessDenied" , "" ) );
                    string[ ] aDArgs = { source.ToString( ) , MethodBase.GetCurrentMethod( ).Name };
                    IAL.singleton.IrcPrivmsg( Configuration.Singleton( ).retrieveGlobalStringOption( "channelDebug" ) , Configuration.Singleton( ).GetMessage( "accessDeniedDebug" , aDArgs ) );
                    return;
                }

                wordResponse = string.Format( wordResponse , args );
                if( rW.action )
                {
                    IAL.singleton.CtcpReply( destination , "ACTION" , wordResponse );
                }
                else
                {
                    IAL.singleton.IrcPrivmsg( destination , wordResponse );
                }
            }

            // Not a word, check for a valid command
            // search for a class that can handle this command.
            Type commandHandler = Type.GetType( "helpmebot6.Commands." + command.Substring( 0 , 1 ).ToUpper( ) + command.Substring( 1 ) );
            if( commandHandler != null )
            {
                ( (Commands.GenericCommand)Activator.CreateInstance( commandHandler ) ).run( source , destination , args );
            }


        }



    }
}
