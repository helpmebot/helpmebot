﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Collections;
    using Castle.Core;
    using MySql.Data.MySqlClient;

    /// <summary>
    /// The LegacyDatabase interface.
    /// </summary>
    public interface ILegacyDatabase : IInitializable
    {
        /// <summary>
        /// Connects this instance to the database.
        /// </summary>
        /// <returns>true if successful</returns>
        bool Connect();

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

        /// <summary>
        /// The execute select.
        /// </summary>
        /// <param name="cmd">
        /// The command.
        /// </param>
        /// <returns>
        /// The <see cref="ArrayList"/>.
        /// </returns>
        ArrayList ExecuteSelect(MySqlCommand cmd);

        /// <summary>
        /// The execute scalar select.
        /// </summary>
        /// <param name="cmd">
        /// The command.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string ExecuteScalarSelect(MySqlCommand cmd);

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <param name="command">
        /// The delete command.
        /// </param>
        void ExecuteCommand(MySqlCommand command);
    }
}