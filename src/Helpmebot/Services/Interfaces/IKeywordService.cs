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

namespace Helpmebot.Services.Interfaces
{
    using Castle.Core;
    using Helpmebot.Model;

    /// <summary>
    /// The KeywordService interface.
    /// </summary>
    public interface IKeywordService : IStartable
    {
        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        void Delete(string name);

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="response">
        /// The content.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        void Create(string name, string response, bool action);

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        Keyword Get(string name);
    }
}