// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationBackgroundService.cs" company="Helpmebot Development Team">
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
//   The notification service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Background
{
    using System.Linq;
    using System.Timers;

    using Castle.Core.Logging;

    using Helpmebot.Background.Interfaces;
    using Helpmebot.Legacy.IRC;
    using Helpmebot.Repositories.Interfaces;

    /// <summary>
    /// The notification service.
    /// </summary>
    public class NotificationBackgroundService : TimerBackgroundServiceBase, INotificationBackgroundService
    {
        /// <summary>
        /// The IRC client.
        /// </summary>
        private readonly IIrcAccessLayer ircClient;

        /// <summary>
        /// The notification repository.
        /// </summary>
        private readonly INotificationRepository notificationRepository;

        /// <summary>
        /// The sync point.
        /// </summary>
        private readonly object syncLock = new object();

        /// <summary>
        /// Initialises a new instance of the <see cref="NotificationBackgroundService"/> class.
        /// </summary>
        /// <param name="ircClient">
        /// The IRC client.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="notificationRepository">
        /// The notification Repository.
        /// </param>
        public NotificationBackgroundService(IIrcAccessLayer ircClient, ILogger logger, INotificationRepository notificationRepository)
            : base(logger, 5 * 1000)
        {
            this.ircClient = ircClient;
            this.notificationRepository = notificationRepository;
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
            this.Logger.Debug("TimerOnElapsed()");

            // Check we aren't already doing a notification check.
            lock (this.syncLock)
            {
                this.Logger.Debug("Retrieving items from notification queue...");

                // Get items from the notification queue
                var list = this.notificationRepository.RetrieveLatest().ToList();

                this.Logger.DebugFormat("Found {0} items.", list.Count());

                // Iterate to send them.
                foreach (var notification in list)
                {
                    var destination = "##helpmebot";

                    // TODO: move me to a separate table or something
                    switch (notification.Type)
                    {
                        case 1:
                            destination = "#wikipedia-en-accounts";
                            break;

                        case 2:
                            destination = "#wikipedia-en-accounts-devs";
                            break;
                    }

                    this.ircClient.IrcPrivmsg(destination, notification.Text);
                }
            }
        }
    }
}
