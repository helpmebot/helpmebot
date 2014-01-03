// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeywordService.cs" company="Helpmebot Development Team">
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
//   Defines the KeywordService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services
{
    using System.Linq;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate.Criterion;

    /// <summary>
    /// The keyword service.
    /// </summary>
    public class KeywordService : IKeywordService
    {
        /// <summary>
        /// The repository.
        /// </summary>
        private readonly IKeywordRepository repository;

        /// <summary>
        /// Initialises a new instance of the <see cref="KeywordService"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        public KeywordService(IKeywordRepository repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public void Delete(string name)
        {
            this.repository.BeginTransaction();
            this.repository.Delete(Restrictions.Eq("Name", name));
            this.repository.Commit();
        }

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
        public void Create(string name, string response, bool action)
        {
            Keyword existing = this.repository.GetByName(name).Any()
                                   ? this.repository.GetByName(name).First()
                                   : new Keyword { Name = name };

            existing.Action = action;
            existing.Response = response;

            this.repository.Save(existing);
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public Keyword Get(string name)
        {
            var existing = this.repository.GetByName(name).ToList();
            if (existing.Any())
            {
                return existing.First();
            }

            return null;
        }
    }
}
