// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResponseRepository.cs" company="Helpmebot Development Team">
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
    using System.Linq;
    using System.Text;

    using Castle.Core.Logging;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using NHibernate;
    using NHibernate.Criterion;

    /// <summary>
    ///     The message repository.
    /// </summary>
    public class ResponseRepository : RepositoryBase<Response>, IResponseRepository
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="ResponseRepository"/> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public ResponseRepository(ISession session, ILogger logger)
            : base(session, logger)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get response.
        /// </summary>
        /// <param name="messageKey">
        /// The message key.
        /// </param>
        /// <returns>
        /// The <see cref="Response"/>.
        /// </returns>
        public Response GetByName(string messageKey)
        {
            return this.Get(Restrictions.Eq("Name", Encoding.UTF8.GetBytes(messageKey))).FirstOrDefault();
        }

        /// <summary>
        /// This rather expensive operation forces NHibernate to re-read all <see cref="Response">Responses</see> from
        /// the database.
        /// </summary>
        public void RefreshAllResponses()
        {
            this.Refresh(this.Get());
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