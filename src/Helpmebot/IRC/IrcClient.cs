// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IrcClient.cs" company="Helpmebot Development Team">
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
//   Defines the IrcClient type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using Helpmebot.IRC.Events;

    /// <summary>
    /// The IRC client.
    /// </summary>
    public class IrcClient
    {

        /// <summary>
        /// The network client.
        /// </summary>
        private readonly NetworkClient networkClient;

        /// <summary>
        /// The data interception function.
        /// </summary>
        private bool connectionRegistered = false;

        /// <summary>
        /// Initialises a new instance of the <see cref="IrcClient"/> class.
        /// </summary>
        /// <param name="hostname">
        /// The hostname.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        public IrcClient(string hostname, int port, string nickname, string username, string realName, string password)
        {
            this.Nickname = nickname;
            this.networkClient = new NetworkClient(hostname, port);
            this.networkClient.DataReceived += this.NetworkClientOnDataReceived;

            this.RegisterConnection(username, realName, password);
        }


        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public string Nickname { get; set; }

        private void NetworkClientOnDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            throw new NotImplementedException();
        }

        private void RegisterConnection(string username, string realName, string password)
        {
            var registrationMessages = new List<string>
                                           {
                                               string.Format("CAP LS"),
                                               string.Format("USER {0} * * :{1}", username, realName),
                                               string.Format("PASS {0}", password),
                                               string.Format("NICK {0}", this.Nickname)
                                           };

            this.networkClient.Send(registrationMessages);
        }

    }
}
