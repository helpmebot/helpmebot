// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserFlagService.cs" company="Helpmebot Development Team">
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
//   Defines the UserFlagService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Stwalkerster.IrcClient.Model.Interfaces;

namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The user flag service.
    /// </summary>
    public class UserFlagService : IUserFlagService
    {
        /// <summary>
        /// The user repository.
        /// </summary>
        private readonly IUserRepository userRepository;

        /// <summary>
        /// Initialises a new instance of the <see cref="UserFlagService"/> class.
        /// </summary>
        /// <param name="userRepository">
        /// The user repository.
        /// </param>
        public UserFlagService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        /// <summary>
        /// The get flags for user.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        public IEnumerable<string> GetFlagsForUser(IUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get flag group.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="FlagGroup"/>.
        /// </returns>
        public FlagGroup GetFlagGroup(string name)
        {
            throw new NotImplementedException();
        }
    }
}
