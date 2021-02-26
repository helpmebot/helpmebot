// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationMap.cs" company="Helpmebot Development Team">
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
//   The notification map.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;

    using Helpmebot.Model;

    /// <summary>
    /// The notification map.
    /// </summary>
    public class NotificationMap : ClassMap<Notification>
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="NotificationMap"/> class.
        /// </summary>
        public NotificationMap()
        {
            this.Table("notification");
            this.Id(x => x.Id).Column("id");
            this.Map(x => x.Text).Column("text");
            this.Map(x => x.Type).Column("type");
            this.Map(x => x.Date).Column("date");
            this.Map(x => x.Handled).Column("handled");
        }
    }
}
