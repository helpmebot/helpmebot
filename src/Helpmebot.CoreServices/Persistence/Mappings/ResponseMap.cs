// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseMap.cs" company="Helpmebot Development Team">
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
    /// The response map.
    /// </summary>
    public class ResponseMap : ClassMap<Response>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="ResponseMap"/> class.
        /// </summary>
        public ResponseMap()
        {
            this.Table("messages");
            this.Id(x => x.Id, "message_id");
            this.Map(x => x.Name, "message_name");
            this.Map(x => x.Text, "message_text");
        }

        #endregion
    }
}