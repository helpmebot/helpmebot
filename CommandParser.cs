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

        public void CommandParser_CommandRecievedEvent( User source, string destination, string command, string[ ] args )
        {
            // if on ignore list, ignore!
            if( source.AccessLevel == User.userRights.Ignored )
                return;


            // Check for a learned word
            string wordResponse =WordLearner.Remember(command);
            if ( wordResponse != string.Empty )
            {
                if( source.AccessLevel < User.userRights.Normal )
                {
                    IAL.singleton.IrcNotice( source.Nickname , Configuration.Singleton( ).GetMessage( "accessDenied" , "" ) );
                    string[ ] aDArgs = { source.ToString( ) , MethodBase.GetCurrentMethod( ).Name };
                    IAL.singleton.IrcPrivmsg( Configuration.Singleton( ).retrieveGlobalStringOption( "channelDebug" ) , Configuration.Singleton( ).GetMessage( "accessDeniedDebug" , aDArgs ) );
                    return;
                }

                wordResponse = string.Format( wordResponse, args );
                IAL.singleton.IrcPrivmsg( destination, wordResponse );
            }

            // Not a word, check for a valid command

            switch ( command )
            {
                case "sayhi":
                    new Commands.SayHi( ).run( source , destination , args  );
                    break;
                case "faq":
                    new Commands.Faq( ).run( source , destination , args   );
                    break;
                case "messagecount":
                    new Commands.MessageCount( ).run( source , destination , args   );
                    break;
                case "set":
                    new Commands.Set( ).run( source , destination , args   );
                    break;
                case "die":
                    new Commands.Die( ).run( source , destination , args   );
                    break;
                case "learn":
                    new Commands.Learn( ).run( source , destination , args );
                    break;
                case "forget":
                    new Commands.Forget( ).run( source , destination , args );
                    break;
                case "join":
                    new Commands.Join( ).run( source , destination , args  );
                    break;
                case "rights":
                    new Commands.Rights( ).run( source , destination , args  );
                    break;
                case "count":
                    new Commands.Count( ).run( source , destination , args  );
                    break;
                case "registration":
                    new Commands.Registration( ).run( source , destination , args  );
                    break;
                case "userinfo":
                    new Commands.UserInfo( ).run( source , destination , args );
                    break;
                case "version":
                    new Commands.Version( ).run( source , destination , args  );
                    break;
                case "maxlag":
                    new Commands.MaxLag( ).run( source , destination , args );
                    break;
                case "time":
                    new Commands.Time( ).run( source , destination , args );
                    break;
                case "age":
                    new Commands.Age( ).run( source , destination , args );
                    break;
                default:
                    break;
            }

            
        }



    }
}
