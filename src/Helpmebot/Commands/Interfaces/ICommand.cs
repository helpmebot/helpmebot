// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommand.cs" company="Helpmebot Development Team">
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
//   The Command interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Stwalkerster.IrcClient.Model.Interfaces;

namespace Helpmebot.Commands.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// The Command interface.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the flag.
        /// </summary>
        string Flag { get; }

        /// <summary>
        /// Gets the command source.
        /// </summary>
        string CommandSource { get; }

        /// <summary>
        /// Gets the user.
        /// </summary>
        IUser User { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        IEnumerable<string> Arguments { get; }

        /// <summary>
        /// The can execute.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool CanExecute();

        /// <summary>
        /// The run.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        IEnumerable<CommandResponse> Run();
    }
}