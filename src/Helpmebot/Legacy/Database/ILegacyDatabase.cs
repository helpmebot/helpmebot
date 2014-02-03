// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILegacyDatabase.cs" company="Helpmebot Development Team">
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
//   Defines the ILegacyDatabase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Legacy.Database
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// The LegacyDatabase interface.
    /// </summary>
    public interface ILegacyDatabase
    {
        /// <summary>
        /// Connects this instance to the database.
        /// </summary>
        /// <returns>true if successful</returns>
        bool Connect();

        /// <summary>
        /// Inserts values the specified table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// The last Inserted Id.
        /// </returns>
        [Obsolete("Don't use - use either repositories or parameterised queries")]
        long Insert(string table, params string[] values);

        /// <summary>
        /// Updates rows in the specified table.
        /// </summary>
        /// <param name="table">
        /// The table.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <param name="limit">
        /// The limit.
        /// </param>
        /// <param name="conditions">
        /// The conditions.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [Obsolete("Don't use - use either repositories or parameterised queries")]
        bool Update(string table, Dictionary<string, string> items, int limit, params LegacyDatabase.WhereConds[] conditions);

        /// <summary>
        /// Executes the select.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>ArrayList of arrays. Each array is one row in the dataset.</returns>
        ArrayList ExecuteSelect(LegacyDatabase.Select query);

        /// <summary>
        /// Executes the select.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="columns">A list of column names</param>
        /// <returns>ArrayList of arrays. Each array is one row in the dataset.</returns>
        ArrayList ExecuteSelect(LegacyDatabase.Select query, out List<string> columns);

        /// <summary>
        /// Executes the scalar select.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A single value as a string</returns>
        string ExecuteScalarSelect(LegacyDatabase.Select query);

        /// <summary>
        /// Call the HMB_GET_LOCAL_OPTION stored procedure.
        /// </summary>
        /// <param name="option">
        /// The option.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ProcHmbGetLocalOption(string option, string channel);
    }
}