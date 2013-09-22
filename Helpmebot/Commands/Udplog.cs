// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Udplog.cs" company="Helpmebot Development Team">
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
//   Defines the Udplog type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot;

    /// <summary>
    /// The UDP log.
    /// </summary>
    internal class Udplog : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Udplog"/> class.
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
        public Udplog(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// The Execute.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length >= 1)
            {
                int port;
                if (int.TryParse(this.Arguments[0], out port))
                {
                    Logger.instance().copyToUdp = port;
                    return new CommandResponseHandler("Set logger to udp://127.0.0.1:" + this.Arguments[0]);
                }

                return new CommandResponseHandler("Not an int.");
            }

            return new CommandResponseHandler(new Message().get("notEnoughParameters"));
        }
    }
}
