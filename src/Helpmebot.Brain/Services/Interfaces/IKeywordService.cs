// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IKeywordService.cs" company="Helpmebot Development Team">
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
//   Defines the IKeywordService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Brain.Services.Interfaces
{
    using System.Collections.Generic;
    using Castle.Core;
    using Helpmebot.Model;

    /// <summary>
    /// The KeywordService interface.
    /// </summary>
    public interface IKeywordService : IStartable
    {

        /// <summary>
        /// Deletes a learned word
        /// </summary>
        /// <param name="name">
        /// The keyword to delete.
        /// </param>
        void Delete(string name);

        /// <summary>
        /// Creates a new learned word
        /// </summary>
        /// <param name="name">
        /// The keyword to store and retrieve with
        /// </param>
        /// <param name="response">
        /// The response to give
        /// </param>
        /// <param name="action">
        /// Flag indicating if this response should be given as a CTCP ACTION
        /// </param>
        void Create(string name, string response, bool action);

        /// <summary>
        /// Retrieves a stored keyword
        /// </summary>
        /// <param name="name">
        /// The keyword to retrieve.
        /// </param>
        /// <returns>
        /// An object representing the keyword
        /// </returns>
        Keyword Get(string name);

        IEnumerable<Keyword> GetAll();
    }
}