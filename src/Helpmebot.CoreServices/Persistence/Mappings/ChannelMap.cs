// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelMap.cs" company="Helpmebot Development Team">
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
    /// The channel map.
    /// </summary>
    public class ChannelMap : ClassMap<Channel>
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ChannelMap"/> class.
        /// </summary>
        public ChannelMap()
        {
            this.Table("channel");
            this.Id(x => x.Id, "channel_id");
            this.Map(x => x.Name, "channel_name");
            this.Map(x => x.Password, "channel_password");
            this.Map(x => x.Enabled, "channel_enabled");
            this.Map(x => x.AutoLink, "autolink");
            this.Map(x => x.Silenced, "silence");
            this.References(x => x.BaseWiki, "basewiki");
            this.Map(x => x.HedgehogMode, "hedgehog");
            this.Map(x => x.WelcomerFlag, "welcomerflag");
        }
    }
}
