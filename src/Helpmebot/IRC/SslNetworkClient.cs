// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SslNetworkClient.cs" company="Helpmebot Development Team">
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
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.IRC
{
    using System.IO;
    using System.Net.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;

    using Castle.Core.Logging;

    /// <summary>
    ///     The SSL network client.
    /// </summary>
    public class SslNetworkClient : NetworkClient
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="SslNetworkClient"/> class.
        /// </summary>
        /// <param name="hostname">
        /// The hostname.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public SslNetworkClient(string hostname, int port, ILogger logger)
            : base(hostname, port, logger, false)
        {
            var sslStream = new SslStream(this.Client.GetStream());

            sslStream.AuthenticateAsClient(hostname, new X509CertificateCollection(), SslProtocols.Tls, false);

            this.Reader = new StreamReader(sslStream);
            this.Writer = new StreamWriter(sslStream);

            this.StartThreads();
        }

        #endregion
    }
}