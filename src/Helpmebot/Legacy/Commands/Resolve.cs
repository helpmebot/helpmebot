﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Resolve.cs" company="Helpmebot Development Team">
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
//   Perform a reverse DNS lookup on an IP address.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Model;

    /// <summary>
    /// Perform a reverse DNS lookup on an IP address.
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Resolve : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Resolve"/> class.
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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Resolve(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var messageService = this.CommandServiceHelper.MessageService;
            if (this.Arguments.Length == 0)
            {
                string[] messageParameters = { "resolve", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture) };
                return new CommandResponseHandler(messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            IPAddress[] addresses = new IPAddress[0];
            try
            {
                addresses = Dns.GetHostEntry(this.Arguments[0]).AddressList;
            }
            catch (SocketException)
            {
            }

            if (addresses.Length != 0)
            {
                string ipList = string.Empty;
                bool first = true;
                foreach (IPAddress item in addresses)
                {
                    if (!first)
                    {
                        ipList += ", ";
                    }

                    ipList += item.ToString();
                    first = false;
                }

                string[] messageargs = { this.Arguments[0], ipList };

                return new CommandResponseHandler(messageService.RetrieveMessage("resolve", this.Channel, messageargs));
            }
            else
            {
                string[] messageargs = { this.Arguments[0] };
                return new CommandResponseHandler(messageService.RetrieveMessage("resolveFail", this.Channel, messageargs));
            }
        }
    }
}
