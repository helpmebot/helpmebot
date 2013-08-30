// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Age.cs" company="Helpmebot Development Team">
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
//   Returns the age of a wikipedian
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;

    /// <summary>
    ///   Returns the age of a wikipedian
    /// </summary>
    internal class Age : GenericCommand
    {
        public Age(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            string userName;
            if (args.Length > 0 && args[0] != "")
            {
                userName = string.Join(" ", args);
            }
            else
            {
                userName = source.nickname;
			}
            TimeSpan time = getWikipedianAge(userName, channel);
            string message;
            if (time.Equals(new TimeSpan(0)))
            {
                string[] messageParameters = { userName };
                message = new Message().get("noSuchUser", messageParameters);
            }
            else
            {
                string[] messageParameters = {
                                                 userName, (time.Days/365).ToString(), (time.Days%365).ToString(),
                                                 time.Hours.ToString(), time.Minutes.ToString(),
                                                 time.Seconds.ToString()
                                             };
                message = new Message().get("cmdAge", messageParameters);
            }
            return new CommandResponseHandler(message);
        }

        /// <summary>
        /// Gets the wikipedian age.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel the command is requested in. (Retrieves the relevant base wiki)</param>
        /// <returns></returns>
        public static TimeSpan getWikipedianAge(string userName, string channel)
        {
            DateTime regdate = Registration.getRegistrationDate(userName, channel);
            TimeSpan age = DateTime.Now.Subtract(regdate);
            if (regdate.Equals(new DateTime(0001, 1, 1)))
            {
                age = new TimeSpan(0);
            }
            return age;
        }
    }
}