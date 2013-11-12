// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Uptime.cs" company="Helpmebot Development Team">
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
//   Gets the uptime of the bot
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// Gets the uptime of the bot
    /// </summary>
    internal class Uptime : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Uptime"/> class.
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
        public Uptime(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The <see cref="CommandResponseHandler"/>.</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] messageParams =
                {
                    Helpmebot6.StartupTime.DayOfWeek.ToString(),
                    Helpmebot6.StartupTime.ToLongDateString(),
                    Helpmebot6.StartupTime.ToLongTimeString()
                };
            string message = this.MessageService.RetrieveMessage("cmdUptimeUpSince", this.Channel, messageParams);
            return new CommandResponseHandler(message);
        }
    }
}