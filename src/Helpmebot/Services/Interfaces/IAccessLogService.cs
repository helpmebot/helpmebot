// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAccessLogService.cs" company="Helpmebot Development Team">
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
//   Defines the IAccessLogService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The AccessLogService interface.
    /// </summary>
    public interface IAccessLogService
    {
        /// <summary>
        /// The success.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        void Success(IUser user, Type command, IEnumerable<string> arguments);

        /// <summary>
        /// The failure.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        void Failure(IUser user, Type command, IEnumerable<string> arguments);
    }
}
