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

    using Helpmebot;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///   Returns the age of a wikipedian
    /// </summary>
    internal class Age : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Age"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Age(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Gets the wikipedian age.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="channel">The channel the command is requested in. (Retrieves the relevant base wiki)</param>
        /// <returns>timespan of the age</returns>
        public static TimeSpan GetWikipedianAge(string userName, string channel)
        {
            DateTime regdate = Registration.GetRegistrationDate(userName, channel);
            TimeSpan age = DateTime.Now.Subtract(regdate);
            if (regdate.Equals(new DateTime(0001, 1, 1)))
            {
                age = new TimeSpan(0);
            }

            return age;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string userName;
            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                userName = string.Join(" ", this.Arguments);
            }
            else
            {
                userName = this.Source.nickname;
            }

            TimeSpan time = GetWikipedianAge(userName, this.Channel);
            string message;
            if (time.Equals(new TimeSpan(0)))
            {
                string[] messageParameters = { userName };
                message = this.MessageService.RetrieveMessage("noSuchUser", this.Channel, messageParameters);
            }
            else
            {
                string[] messageParameters =
                    {
                        userName, (time.Days / 365).ToString(), (time.Days % 365).ToString(),
                        time.Hours.ToString(), time.Minutes.ToString(), time.Seconds.ToString()
                    };
                message = this.MessageService.RetrieveMessage("cmdAge", this.Channel, messageParameters);
            }

            return new CommandResponseHandler(message);
        }
    }
}
