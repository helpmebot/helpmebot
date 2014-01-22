// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryWatcher.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Model
{
    using Helpmebot.Persistence;

    /// <summary>
    /// The category watcher.
    /// </summary>
    public class CategoryWatcher : EntityBase
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the category.
        /// </summary>
        public virtual string Category { get; set; }

        /// <summary>
        ///     Gets or sets the keyword.
        /// </summary>
        public virtual string Keyword { get; set; }

        /// <summary>
        ///     Gets or sets the priority.
        /// </summary>
        public virtual int Priority { get; set; }

        /// <summary>
        ///     Gets or sets the sleep time.
        /// </summary>
        public virtual int SleepTime { get; set; }

        #endregion
    }
}