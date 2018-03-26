// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationRepository.cs" company="Helpmebot Development Team">
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
//   The notification repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using NHibernate.Util;

namespace Helpmebot.Repositories
{
    using System.Collections.Generic;

    using Castle.Core.Internal;
    using Castle.Core.Logging;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using NHibernate;

    /// <summary>
    /// The notification repository.
    /// </summary>
    public class NotificationRepository : RepositoryBase<Notification>, INotificationRepository
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="NotificationRepository"/> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public NotificationRepository(ISession session, ILogger logger)
            : base(session, logger)
        {
        }

        /// <summary>
        /// Retrieves the latest notifications from the database, and then removes them from the database.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Notification}"/>.
        /// </returns>
        public IEnumerable<Notification> RetrieveLatest()
        {
            IEnumerable<Notification> list = new List<Notification>();

            this.Transactionally(
                session =>
                    {
                        list = session.CreateCriteria<Notification>().List<Notification>();
                        list.ForEach(session.Delete);
                    });

            return list;
        }
    }
}
