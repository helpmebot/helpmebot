// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Part.cs" company="Helpmebot Development Team">
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
//   Leave an IRC channel
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Collections.Generic;

    using Helpmebot;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// Leave an IRC channel
    /// </summary>
    internal class Part : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Part"/> class.
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
        public Part(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            Helpmebot6.irc.IrcPart(this.Channel, this.Source.ToString());
            Dictionary<string, string> vals = new Dictionary<string, string> { { "channel_enabled", "0" } };
            DAL.singleton()
                .update(
                    "channel",
                    vals,
                    0,
                    new DAL.WhereConds("channel_name", this.Channel),
                    new DAL.WhereConds("channel_network", this.Source.Network.ToString()));
            return null;
        }
    }
}