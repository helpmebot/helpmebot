// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Notification.cs" company="Helpmebot Development Team">
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
//   Defines the Notification type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Model
{
    using System;

    using Helpmebot.Persistence;

    /// <summary>
    /// The notification.
    /// </summary>
    public class Notification : EntityBase
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public virtual DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public virtual string Type { get; set; }
        
        public virtual bool Handled { get; set; }
    }
}
