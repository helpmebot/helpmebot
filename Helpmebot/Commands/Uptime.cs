﻿// --------------------------------------------------------------------------------------------------------------------
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
    /// <summary>
    /// Gets the uptime of the bot
    /// </summary>
    internal class Uptime : GenericCommand
    {
        public Uptime(User source, string channel, string[] args)
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
            string[] messageParams = {
                                         Helpmebot6.StartupTime.DayOfWeek.ToString(),
                                         Helpmebot6.StartupTime.ToLongDateString(),
                                         Helpmebot6.StartupTime.ToLongTimeString()
                                     };
            string message = new Message().get("cmdUptimeUpSince", messageParams);
            return new CommandResponseHandler(message);
        }
    }
}