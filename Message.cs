// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpmebot6
{
    using System.Collections;

    internal class Message
    {
        private readonly DAL _dbal;

        public Message( )
        {
            _dbal = DAL.singleton( );
        }
        
        private string[] getMessages(string messageName)
        {
            // normalise message name to account for old messages
            if (messageName.Substring(0, 1).ToUpper() != messageName.Substring(0, 1))
            {
                messageName = messageName.Substring(0, 1).ToUpper() + messageName.Substring(1);
            }

            //get message text from database
            string messageText = _dbal.proc_HMB_GET_MESSAGE_CONTENT(messageName);

            // split up lines and pass back arraylist

            return messageText.Split('\n');
        }

        //returns a random message chosen from the list of possible message names
        
        private string chooseRandomMessage(string messageName)
        {
            Random rnd = new Random();
            string[] al = getMessages(messageName);
            if (al.Length == 0)
            {
                Helpmebot6.irc.ircPrivmsg(Helpmebot6.debugChannel,
                                          "***ERROR*** Message '" + messageName + "' not found in message table");
                return "";
            }
            return al[rnd.Next(0, al.Length)].ToString();
        }


        private static string buildMessage(string messageFormat, params string[] args)
        {
            return String.Format(messageFormat, args);
        }

        
        public string get(string messageName)
        {
            return chooseRandomMessage(messageName);
        }
        
        public string get(string messageName, params string[] args)
        {

            return buildMessage(chooseRandomMessage(messageName), args);
        }
    }
}
