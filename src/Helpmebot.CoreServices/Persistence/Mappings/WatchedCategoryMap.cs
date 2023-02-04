// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WatchedCategoryMap.cs" company="Helpmebot Development Team">
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
    ///     The category watcher map.
    /// </summary>
    public class WatchedCategoryMap : ClassMap<WatchedCategory>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initialises a new instance of the <see cref="WatchedCategoryMap" /> class.
        /// </summary>
        public WatchedCategoryMap()
        {
            this.Table("watcher");
            this.Id(x => x.Id, "watcher_id");

            this.Map(x => x.Category, "category");
            this.Map(x => x.Keyword, "keyword");
            this.Map(x => x.BaseWikiId, "base_wiki_id");

            this.HasMany(x => x.Channels).Cascade.AllDeleteOrphan().Table("channelwatchers").Inverse();
            this.HasMany(x => x.CategoryItems).Cascade.AllDeleteOrphan().Table("categoryitems").Inverse();
        }

        #endregion
    }
}