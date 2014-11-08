// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeywordRepository.cs" company="Helpmebot Development Team">
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
//   Defines the KeywordRepository type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using NHibernate;
    using NHibernate.Criterion;

    /// <summary>
    /// The keyword repository.
    /// </summary>
    public class KeywordRepository : RepositoryBase<Keyword>, IKeywordRepository
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="KeywordRepository"/> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public KeywordRepository(ISession session, ILogger logger)
            : base(session, logger)
        {
        }

        /// <summary>
        /// The get by name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="Keyword"/>.
        /// </returns>
        public IEnumerable<Keyword> GetByName(string name)
        {
            return this.Get(Restrictions.Eq("Name", name));
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        public void Create(string name, string response, bool action)
        {
            this.Transactionally(
                session =>
                    {
                        var existing =
                            session.CreateCriteria<Keyword>()
                                .Add(Restrictions.Eq("Name", name))
                                .List<Keyword>()
                                .FirstOrDefault() ?? new Keyword();

                        existing.Name = name;
                        existing.Response = response;
                        existing.Action = action;

                        session.SaveOrUpdate(existing);
                    });
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(true);
        }
    }
}
