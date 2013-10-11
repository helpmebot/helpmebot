// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Message.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Defines the Message type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;

    using Helpmebot.IRC.Legacy;

    internal class Message
    {
        private readonly DAL _dbal;

        public Message( )
        {
            this._dbal = DAL.singleton( );
        }
        
        private string[] getMessages(string messageName)
        {
            //get message text from database
            string messageText = this._dbal.proc_HMB_GET_MESSAGE_CONTENT(messageName);

            // split up lines and pass back arraylist

            string[] messages = messageText.Split('\n');
            
            return messages;
        }

        //returns a random message chosen from the list of possible message names
        
        private string chooseRandomMessage(string messageName)
        {
            // normalise message name to account for old messages
            if (messageName.Substring(0, 1).ToUpper() != messageName.Substring(0, 1))
            {
                messageName = messageName.Substring(0, 1).ToUpper() + messageName.Substring(1);
            }

            Random rnd = new Random();
            string[] al = this.getMessages(Configuration.singleton()["messagePrefix"]+ messageName);
            if (al.Length == 0) // no messages found with prefix
            {
                if (Configuration.singleton()["messagePrefix"] != string.Empty) // check a prefix was added
                {
                    Helpmebot6.irc.IrcPrivmsg(Helpmebot6.debugChannel,
                                              "***ERROR*** Message '" + messageName + "' not found with prefix '" +
                                              Configuration.singleton()["messagePrefix"] +
                                              "'. Attempting without prefix...");
                    // remove prefix and retry

                    al = this.getMessages(messageName);
                }

                if (al.Length == 0) // still nothing there
                {

                    // error out - can't find message
                    Helpmebot6.irc.IrcPrivmsg(Helpmebot6.debugChannel,
                                              "***ERROR*** Message '" + messageName + "' not found in message table");
                    return "";
                }
            }
            return al[rnd.Next(0, al.Length)];
        }


        private static string buildMessage(string messageFormat, params string[] args)
        {
            string builtString = String.Format(messageFormat, args);
            if(builtString.StartsWith("#ACTION"))
            {
                builtString = IrcAccessLayer.WrapCTCP("ACTION", builtString.Substring(8));
            }
            return builtString;
        }

        
        public string get(string messageName)
        {
            return this.get(messageName, new string[0]);
        }
        
        public string get(string messageName, params string[] args)
        {
            return buildMessage(this.chooseRandomMessage(messageName), args);
        }
    }
}
