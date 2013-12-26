// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeepAliveService.cs" company="Helpmebot Development Team">
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
//   Defines the KeepAliveService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Background
{
    using System;
    using System.Threading;

    using Castle.Core.Logging;

    using Helpmebot.Background.Interfaces;
    using Helpmebot.Legacy.IRC;

    /// <summary>
    /// A silly service to test the background service stuff.
    /// </summary>
    public class KeepAliveService : IKeepAliveService
    {
        /// <summary>
        /// The IRC client.
        /// </summary>
        private readonly IIrcAccessLayer ircClient;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The background thread.
        /// </summary>
        private readonly Thread backgroundThread;

        /// <summary>
        /// Initialises a new instance of the <see cref="KeepAliveService"/> class.
        /// </summary>
        /// <param name="ircClient">
        /// The IRC client.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public KeepAliveService(IIrcAccessLayer ircClient, ILogger logger)
        {
            this.ircClient = ircClient;
            this.logger = logger;
            this.logger.Info("Initialising keepalive service");
            this.backgroundThread = new Thread(this.ThreadBody);
        }

        /// <summary>
        /// The start.
        /// </summary>
        public void Start()
        {
            this.logger.Info("Starting keepalive service");
            this.backgroundThread.Start();
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public void Stop()
        {
            this.logger.Info("Stopping keepalive service");
            this.backgroundThread.Abort();
        }

        /// <summary>
        /// The thread body.
        /// </summary>
        private void ThreadBody()
        {
            while (true)
            {
                Thread.Sleep(60 * 1000); // ping every minute.
                this.ircClient.IrcPrivmsg("##stwalkerster", "PING");
            }
        }
    }
}
