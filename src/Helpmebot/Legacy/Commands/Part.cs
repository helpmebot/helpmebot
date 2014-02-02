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
    using Helpmebot;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

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
            // FIXME: servicelocator call
            var channelRepo = ServiceLocator.Current.GetInstance<IChannelRepository>();

            var channel = channelRepo.GetByName(this.Channel);
            channel.Enabled = false;
            channelRepo.Save(channel);

            Helpmebot6.irc.IrcPart(this.Channel, this.Source.ToString());

            return null;
        }
    }
}