// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WelcomeUserRepository.cs" company="Helpmebot Development Team">
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
//   Defines the WelcomeUserRepository type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories
{
    using System.Collections.Generic;

    using Castle.Core.Logging;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using NHibernate;
    using NHibernate.Criterion;

    /// <summary>
    /// The welcome user repository.
    /// </summary>
    public class WelcomeUserRepository : RepositoryBase<WelcomeUser>, IWelcomeUserRepository
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="WelcomeUserRepository"/> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public WelcomeUserRepository(ISession session, ILogger logger)
            : base(session, logger)
        {
        }

        /// <summary>
        /// The get welcome for channel.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{WelcomeUser}"/>.
        /// </returns>
        public IEnumerable<WelcomeUser> GetWelcomeForChannel(string channel)
        {
            return
                this.Session.CreateCriteria<WelcomeUser>().Add(Restrictions.Eq("Channel", channel)).List<WelcomeUser>();
        }

        /// <summary>
        /// The get exceptions for channel.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{WelcomeUser}"/>.
        /// </returns>
        public IEnumerable<WelcomeUser> GetExceptionsForChannel(string channel)
        {
            throw new System.NotImplementedException();
        }
    }
}
