// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Whois.cs" company="Helpmebot Development Team">
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
//   The whois.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    using global::Whois.NET;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;

    using Helpmebot.IRC.Model;
    using Helpmebot.Model;

    /// <summary>
    /// The whois.
    /// </summary>
    internal class Whois : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Whois"/> class.
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
        /// The command Service Helper.
        /// </param>
        public Whois(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        ///     The execute command.
        /// </summary>
        /// <returns>
        ///     The <see cref="CommandResponseHandler" />.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var messageService = this.CommandServiceHelper.MessageService;
            if (this.Arguments.Length >= 1)
            {
                var ip = this.GetIPAddress();
                if (ip == null)
                {
                    return new CommandResponseHandler("Unable to find IP address to query.");
                }

                var whoisResult = WhoisClient.Query(ip.ToString());

                var orgname = whoisResult.OrganizationName ?? "(unknown)";
                var range = whoisResult.AddressRange == null
                                ? "(unknown)"
                                : string.Format(
                                    "{0} - {1}",
                                    whoisResult.AddressRange.Begin,
                                    whoisResult.AddressRange.End);

                var msg = string.Format("Whois for {2} (requested {0}) gives organisation {1}", ip, orgname, range);

                return new CommandResponseHandler(msg);
            }
            else
            {
                string[] messageParameters = { "whois", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture) };
                this.CommandServiceHelper.Client.SendNotice(
                    this.Source.Nickname,
                    messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            return null;
        }

        private IPAddress GetIPAddress()
        {
            IPAddress ip;
            if (IPAddress.TryParse(this.Arguments[0], out ip))
            {
                return ip;
            }
            
            var match = Regex.Match(this.Arguments[0], "^[a-fA-F0-9]{8}$");
            if (match.Success)
            {
                // We've got a hex-encoded IP.
                return Decode.GetIpAddressFromHex(this.Arguments[0]);
            }

            IrcUser ircUser;
            if (this.CommandServiceHelper.Client.UserCache.TryGetValue(this.Arguments[0], out ircUser))
            {
                var usermatch = Regex.Match(ircUser.Username, "^[a-fA-F0-9]{8}$");
                if (usermatch.Success)
                {
                    // We've got a hex-encoded IP.
                    return Decode.GetIpAddressFromHex(ircUser.Username);
                }

                if (!ircUser.Hostname.Contains("/"))
                {
                    // real hostname, not a cloak
                    var hostAddresses = Dns.GetHostAddresses(ircUser.Hostname);
                    if (hostAddresses.Length > 0)
                    {
                        return hostAddresses.First();
                    }
                }
            }
            
            return null;
        }
    }
}