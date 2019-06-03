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
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Timers;
    using Castle.Core.Logging;
    using Stwalkerster.IrcClient.Interfaces;
    using Helpmebot.Background.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Util;

    /// <summary>
    /// The notification service.
    /// </summary>
    public class NotificationBackgroundService : TimerBackgroundServiceBase, INotificationBackgroundService
    {
        private readonly IIrcClient ircClient;
        private readonly ISession session;
        private readonly BotConfiguration configuration;

        /// <summary>
        /// The sync point.
        /// </summary>
        private readonly object syncLock = new object();

        public NotificationBackgroundService(IIrcClient ircClient, ILogger logger, ISession session, BotConfiguration configuration)
            : base(logger, 5 * 1000, configuration.EnableNotificationService)
        {
            this.ircClient = ircClient;
            this.session = session;
            this.configuration = configuration;
        }

        protected override void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            this.Logger.Debug("TimerOnElapsed()");

            // Check we aren't already doing a notification check.
            lock (this.syncLock)
            {
                this.Logger.Debug("Retrieving items from notification queue...");

                var transaction = this.session.BeginTransaction(IsolationLevel.RepeatableRead);
                if (!transaction.IsActive)
                {
                    this.Logger.Error("Could not start transaction in notification service");
                    return;
                }

                IList<Notification> list;
                IList<NotificationType> types;

                // Get items from the notification queue
                try
                {

                    list = this.session.CreateCriteria<Notification>().List<Notification>();
                    list.ForEach(this.session.Delete);

                    types = this.session.CreateCriteria<NotificationType>().List<NotificationType>();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    this.Logger.Error("Exception during retrive of notifications");
                    transaction.Rollback();
                    throw;
                }

                this.Logger.DebugFormat("Found {0} items.", list.Count);

                // Iterate to send them.
                foreach (var notification in list)
                {
                    var destinations = types.Where(x => x.Type == notification.Type).Select(x => x.Channel.Name).ToList();

                    if (destinations.Any())
                    {
                        destinations.ForEach(
                            x => this.ircClient.SendMessage(x, this.SanitiseMessage(notification.Text)));
                    }
                    else
                    {
                        this.ircClient.SendMessage(
                            this.configuration.DebugChannel,
                            this.SanitiseMessage(notification.Text));
                    }
                }
            }
        }

        private string SanitiseMessage(string text)
        {
            return text.Replace("\r", "").Replace("\n", "");
        }
    }
}
