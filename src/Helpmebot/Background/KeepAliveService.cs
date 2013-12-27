﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Timers;

    using Castle.Core.Logging;

    using Helpmebot.Background.Interfaces;
    using Helpmebot.Legacy.IRC;

    /// <summary>
    /// A silly service to test the background service stuff.
    /// </summary>
    public class KeepAliveService : TimerBackgroundServiceBase, IKeepAliveService
    {
        /// <summary>
        /// The IRC client.
        /// </summary>
        private readonly IIrcAccessLayer ircClient;

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
            : base(logger, 60 * 1000)
        {
            this.ircClient = ircClient;
        }

        /// <summary>
        /// The timer on elapsed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="elapsedEventArgs">
        /// The elapsed event args.
        /// </param>
        protected override void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            this.ircClient.IrcPrivmsg("##stwalkerster", "PING timer!");
        }
    }
}
