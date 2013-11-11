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

    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.IRC;

    /// <summary>
    /// The message.
    /// </summary>
    internal class Message
    {
        /// <summary>
        /// The database access layer.
        /// </summary>
        private readonly DAL database;

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message()
        {
            this.database = DAL.singleton();
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="messageName">
        /// The message name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetMessage(string messageName)
        {
            return this.GetMessage(messageName, new string[0]);
        }

        /// <summary>
        /// The get message.
        /// </summary>
        /// <param name="messageName">
        /// The message name.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetMessage(string messageName, params string[] args)
        {
            return BuildMessage(this.ChooseRandomMessage(messageName), args);
        }

        /// <summary>
        /// The build message.
        /// </summary>
        /// <param name="messageFormat">
        /// The message format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string BuildMessage(string messageFormat, params object[] args)
        {
            string builtString = string.Format(messageFormat, args);

            if (builtString.StartsWith("#ACTION"))
            {
                builtString = IrcAccessLayer.WrapCTCP("ACTION", builtString.Substring(8));
            }

            return builtString;
        }

        /// <summary>
        /// The get messages.
        /// </summary>
        /// <param name="messageName">
        /// The message name.
        /// </param>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        private string[] GetMessages(string messageName)
        {
            // get message text from database
            string messageText = this.database.proc_HMB_GET_MESSAGE_CONTENT(messageName);

            // split up lines and pass back arraylist
            string[] messages = messageText.Split('\n');
            
            return messages;
        }

        /// <summary>
        /// Returns a random message chosen from the list of possible message names
        /// </summary>
        /// <param name="messageName">
        /// The message name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ChooseRandomMessage(string messageName)
        {
            // normalise message name to account for old messages
            if (messageName.Substring(0, 1).ToUpper() != messageName.Substring(0, 1))
            {
                messageName = messageName.Substring(0, 1).ToUpper() + messageName.Substring(1);
            }

            var rnd = new Random();
            string[] al = this.GetMessages(LegacyConfig.singleton()["messagePrefix"] + messageName);
            if (al.Length == 0)
            {
                // no messages found with prefix - check a prefix was added
                if (LegacyConfig.singleton()["messagePrefix"] != string.Empty)
                {
                    string message = "***ERROR*** Message '" + messageName + "' not found with prefix '"
                                     + LegacyConfig.singleton()["messagePrefix"] + "'. Attempting without prefix...";
                    Helpmebot6.irc.IrcPrivmsg(
                        Helpmebot6.debugChannel,
                        message);

                    // remove prefix and retry
                    al = this.GetMessages(messageName);
                }

                // still nothing there?
                if (al.Length == 0) 
                {
                    // error out - can't find message
                    string message = "***ERROR*** Message '" + messageName + "' not found in message table";
                    Helpmebot6.irc.IrcPrivmsg(Helpmebot6.debugChannel, message);
                    return string.Empty;
                }
            }

            return al[rnd.Next(0, al.Length)];
        }
    }
}
