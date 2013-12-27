// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUser.cs" company="Helpmebot Development Team">
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
//   The User interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Model.Interfaces
{
    /// <summary>
    /// The User interface.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        /// <value>The nickname.</value>
        string Nickname { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets the hostname.
        /// </summary>
        /// <value>The hostname.</value>
        string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        string Account { get; set; }
    }
}