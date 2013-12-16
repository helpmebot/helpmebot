// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Time.cs" company="Helpmebot Development Team">
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
//   Returns the current date/time
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;

    using Helpmebot;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///   Returns the current date/time
    /// </summary>
    internal class Time : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Time"/> class.
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
        public Time(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] messageParams =
                {
                    this.Source.nickname, DateTime.Now.DayOfWeek.ToString(),
                    DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString("00"),
                    DateTime.Now.Day.ToString("00"), DateTime.Now.Hour.ToString("00"),
                    DateTime.Now.Minute.ToString("00"), DateTime.Now.Second.ToString("00")
                };
            string message = this.MessageService.RetrieveMessage("cmdTime", this.Channel, messageParams);
            return new CommandResponseHandler(message);
        }
    }
}
