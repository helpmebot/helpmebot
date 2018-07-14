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
    using Castle.Core;
    using MySql.Data.MySqlClient;

    public interface ILegacyDatabase : IInitializable
    {
        string ExecuteScalarSelect(MySqlCommand cmd);
        void ExecuteCommand(MySqlCommand command);
    }
}