// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaWikiSiteMap.cs" company="Helpmebot Development Team">
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
    /// The media wiki site map.
    /// </summary>
    public class MediaWikiSiteMap : ClassMap<MediaWikiSite>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="MediaWikiSiteMap"/> class.
        /// </summary>
        public MediaWikiSiteMap()
        {
            this.Table("site");
            this.Id(x => x.Id, "site_id");
            this.Map(x => x.MainPage, "site_mainpage");
            this.Map(x => x.Username, "site_username");
            this.Map(x => x.Password, "site_password");
            this.Map(x => x.Database, "site_database");
            this.Map(x => x.Shard, "site_dbserver");
            this.Map(x => x.Api, "site_api");
        }

        #endregion
    }
}