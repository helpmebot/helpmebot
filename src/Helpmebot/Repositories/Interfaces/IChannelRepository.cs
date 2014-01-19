// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IChannelRepository.cs" company="Helpmebot Development Team">
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
//   The ChannelRepository interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories.Interfaces
{
    using System.Collections.Generic;

    using Helpmebot.Model;

    /// <summary>
    /// The ChannelRepository interface.
    /// </summary>
    public interface IChannelRepository : IRepository<Channel>
    {
        /// <summary>
        /// The get by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="Channel"/>.
        /// </returns>
        Channel GetByName(string name);

        /// <summary>
        /// The get by name.
        /// </summary>
        /// <returns>
        /// The <see cref="Channel"/>.
        /// </returns>
        IEnumerable<Channel> GetEnabled();
    }
}