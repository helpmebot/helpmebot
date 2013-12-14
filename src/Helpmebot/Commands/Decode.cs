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

    using Helpmebot;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// Decodes a hex-encoded IP address
    /// </summary>
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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Decode(User source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length == 0)
            {
                string[] messageParameters = { "decode", "1", this.Arguments.Length.ToString() };
                return new CommandResponseHandler(this.MessageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            if (this.Arguments[0].Length != 8)
            {
                return null;
            }

            byte[] ip = new byte[4];
            ip[0] = Convert.ToByte(this.Arguments[0].Substring(0, 2), 16);
            ip[1] = Convert.ToByte(this.Arguments[0].Substring(2, 2), 16);
            ip[2] = Convert.ToByte(this.Arguments[0].Substring(4, 2), 16);
            ip[3] = Convert.ToByte(this.Arguments[0].Substring(6, 2), 16);

            IPAddress ipAddr = new IPAddress(ip);

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
                string[] messageargs = { this.Arguments[0], ipAddr.ToString(), hostname };
                return new CommandResponseHandler(this.MessageService.RetrieveMessage("hexDecodeResult", this.Channel, messageargs));
            }
            else
            {
                string[] messageargs = { this.Arguments[0], ipAddr.ToString() };
                return new CommandResponseHandler(this.MessageService.RetrieveMessage("hexDecodeResultNoResolve", this.Channel, messageargs));
            }
        }
    }
}