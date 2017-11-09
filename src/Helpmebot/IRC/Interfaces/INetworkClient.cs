// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INetworkClient.cs" company="Helpmebot Development Team">
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
//   Defines the INetworkClient type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IRC.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Helpmebot.IRC.Events;

    /// <summary>
    /// The NetworkClient interface.
    /// </summary>
    public interface INetworkClient : IDisposable
    {
        /// <summary>
        /// The data received.
        /// </summary>
        event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Gets the hostname.
        /// </summary>
        string Hostname { get; }

        /// <summary>
        /// Gets the port.
        /// </summary>
        int Port { get; }
        
        bool Connected { get; }

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Send(string message);

        /// <summary>
        /// The send.
        /// </summary>
        /// <param name="messages">
        /// The messages.
        /// </param>
        void Send(IEnumerable<string> messages);

        /// <summary>
        /// The disconnect.
        /// </summary>
        void Disconnect();
    }
}