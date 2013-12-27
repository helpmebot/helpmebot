// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INotificationRepository.cs" company="Helpmebot Development Team">
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
//   Defines the INotificationRepository type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories.Interfaces
{
    using System.Collections.Generic;

    using Helpmebot.Model;

    /// <summary>
    /// The NotificationRepository interface.
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// Retrieves the latest notifications from the database, and then removes them from the database.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Notification}"/>.
        /// </returns>
        IEnumerable<Notification> RetrieveLatest();
    }
}