// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Decode.cs" company="Helpmebot Development Team">
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
//   Decodes a hex-encoded IP address
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;

    using Helpmebot;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// Decodes a hex-encoded IP address
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Decode : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Decode"/> class.
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
        public Decode(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
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
                string[] messageParameters = { "decode", "1", this.Arguments.Length.ToString() };
                return new CommandResponseHandler(messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            var validHexIp = new Regex("^[0-9A-Fa-f]{8}$");

            var input = this.Arguments[0];

            if (!validHexIp.Match(input).Success)
            {
                return new CommandResponseHandler(messageService.RetrieveMessage("DecodeBadInput", this.Channel, new string[0]));
            }

            var ipAddr = GetIpAddressFromHex(input);

            string hostname = string.Empty;

            try
            {
                hostname = Dns.GetHostEntry(ipAddr).HostName;
            }
            catch (SocketException)
            {
            }

            if (hostname != string.Empty)
            {
                string[] messageargs = { input, ipAddr.ToString(), hostname };
                return new CommandResponseHandler(messageService.RetrieveMessage("hexDecodeResult", this.Channel, messageargs));
            }
            else
            {
                string[] messageargs = { input, ipAddr.ToString() };
                return new CommandResponseHandler(messageService.RetrieveMessage("hexDecodeResultNoResolve", this.Channel, messageargs));
            }
        }

        /// <summary>
        /// The get ip address from hex.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="IPAddress"/>.
        /// </returns>
        public static IPAddress GetIpAddressFromHex(string input)
        {
            var ip = new byte[4];
            ip[0] = Convert.ToByte(input.Substring(0, 2), 16);
            ip[1] = Convert.ToByte(input.Substring(2, 2), 16);
            ip[2] = Convert.ToByte(input.Substring(4, 2), 16);
            ip[3] = Convert.ToByte(input.Substring(6, 2), 16);

            var ipAddr = new IPAddress(ip);
            return ipAddr;
        }
    }
}
