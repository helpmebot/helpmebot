// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILegacyUser.cs" company="Helpmebot Development Team">
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

namespace Helpmebot.Legacy.Model
{
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The User interface.
    /// </summary>
    public interface ILegacyUser : IUser
    {
        /// <summary>
        /// Gets or sets the network.
        /// </summary>
        /// <value>The network.</value>
        uint network { get; }

        /// <summary>
        /// Gets or sets the access level.
        /// </summary>
        /// <value>The access level.</value>
        LegacyUser.UserRights accessLevel { get; set; }
    }
}