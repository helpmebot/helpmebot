// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IgnoredPageMap.cs" company="Helpmebot Development Team">
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

namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;

    using Helpmebot.Model;

    /// <summary>
    /// The ignored page map.
    /// </summary>
    public class CategoryWatcherIgnoredPageMap : ClassMap<CategoryWatcherIgnoredPage>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CategoryWatcherIgnoredPageMap"/> class.
        /// </summary>
        public CategoryWatcherIgnoredPageMap()
        {
            this.Table("catwatcher_ignoredpage");
            this.Id(x => x.Id, "id");
            this.Map(x => x.Title, "title");
        }

        #endregion
    }
}