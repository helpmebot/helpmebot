// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterwikiPrefixMap.cs" company="Helpmebot Development Team">
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
    ///     The inter-wiki prefix map.
    /// </summary>
    internal class InterwikiPrefixMap : ClassMap<InterwikiPrefix>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="InterwikiPrefixMap"/> class.
        /// </summary>
        public InterwikiPrefixMap()
        {
            this.Table("interwikiprefix");
            this.Id(x => x.Id, "id");
            this.Map(x => x.Prefix, "prefix").Unique();
            this.Map(x => x.ImportedAs, "importedas");
            this.Map(x => x.Url, "url");
            this.Map(x => x.AbsentFromLastImport, "absentfromlast");
            this.Map(x => x.CreatedSinceLast, "createdsincelast");
        }

        #endregion
    }
}