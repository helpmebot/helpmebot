﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeywordMap.cs" company="Helpmebot Development Team">
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
//   Defines the KeywordMap type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;

    using Helpmebot.Model;

    /// <summary>
    /// The keyword map.
    /// </summary>
    public class KeywordMap : ClassMap<Keyword>
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="KeywordMap"/> class.
        /// </summary>
        public KeywordMap()
        {
            this.Table("keywords");
            this.Id(keyword => keyword.Id).Column("keyword_id");
            this.Map(k => k.Name).Column("keyword_name");
            this.Map(k => k.Response).Column("keyword_response");
            this.Map(k => k.Action).Column("keyword_action");
        }
    }
}
