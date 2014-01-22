// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IgnoredPagesRepository.cs" company="Helpmebot Development Team">
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
    ///     The ignored pages repository.
    /// </summary>
    public class IgnoredPagesRepository : RepositoryBase<IgnoredPage>, IIgnoredPagesRepository
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="IgnoredPagesRepository"/> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public IgnoredPagesRepository(ISession session, ILogger logger)
            : base(session, logger)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add page.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        public void AddPage(string page)
        {
            if (!this.Get(Restrictions.Eq("Title", page)).Any())
            {
                this.Save(new IgnoredPage { Title = page });
            }
        }

        /// <summary>
        /// The delete page.
        /// </summary>
        /// <param name="page">
        /// The page.
        /// </param>
        public void DeletePage(string page)
        {
            this.Delete(Restrictions.Eq("Title", page));
        }

        /// <summary>
        ///     The get ignored pages.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerable{String}" />.
        /// </returns>
        public IEnumerable<string> GetIgnoredPages()
        {
            return this.Get().Select(x => x.Title);
        }

        #endregion

        #region Methods

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

        #endregion
    }
}