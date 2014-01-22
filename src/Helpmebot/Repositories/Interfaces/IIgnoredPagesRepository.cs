// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIgnoredPagesRepository.cs" company="Helpmebot Development Team">
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
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories.Interfaces
{
    using System.Collections;
    using System.Collections.Generic;

    using Helpmebot.Model;

    /// <summary>
    /// The IgnoredPagesRepository interface.
    /// </summary>
    public interface IIgnoredPagesRepository : IRepository<IgnoredPage>
    {
        /// <summary>
        /// The get ignored pages.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        IEnumerable<string> GetIgnoredPages();

        /// <summary>
        /// The delete page.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        void DeletePage(string page);

        /// <summary>
        /// The add page.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        void AddPage(string page);
    }
}