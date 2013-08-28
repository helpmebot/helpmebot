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

    /// <summary>
    /// Decodes a hex-encoded IP address
    /// </summary>
    internal class Decode : GenericCommand
    {
        public Decode(User source, string channel, string[] args)
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
            if(args.Length == 0)
            {
                string[] messageParameters = { "decode", "1", args.Length.ToString() };
                return new CommandResponseHandler(new Message().get("notEnoughParameters", messageParameters));
            }


            if (args[0].Length != 8)
                return null;

            byte[] ip = new byte[4];
            ip[0] = Convert.ToByte(args[0].Substring(0, 2), 16);
            ip[1] = Convert.ToByte(args[0].Substring(2, 2), 16);
            ip[2] = Convert.ToByte(args[0].Substring(4, 2), 16);
            ip[3] = Convert.ToByte(args[0].Substring(6, 2), 16);

            IPAddress ipAddr = new IPAddress(ip);

            string hostname = "";
            try
            {
                hostname = Dns.GetHostEntry(ipAddr).HostName;
            }
            catch (SocketException)
            {
            }
            if (hostname != string.Empty)
            {
                string[] messageargs = {args[0], ipAddr.ToString(), hostname};
                return new CommandResponseHandler(new Message().get("hexDecodeResult", messageargs));
            }
            else
            {
                string[] messageargs = {args[0], ipAddr.ToString()};
                return
                    new CommandResponseHandler(new Message().get("hexDecodeResultNoResolve",
                                                                                    messageargs));
            }
        }
    }
}