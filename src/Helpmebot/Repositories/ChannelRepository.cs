// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelRepository.cs" company="Helpmebot Development Team">
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
    /// The channel repository.
    /// </summary>
    public class ChannelRepository : RepositoryBase<Channel>, IChannelRepository
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ChannelRepository"/> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public ChannelRepository(ISession session, ILogger logger)
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
        /// The <see cref="Channel"/>.
        /// </returns>
        public Channel GetByName(string name)
        {
            return this.Get(Restrictions.Eq("Name", name)).FirstOrDefault();
        }

        /// <summary>
        /// The get enabled.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{Channel}"/>.
        /// </returns>
        public IEnumerable<Channel> GetEnabled()
        {
            return this.Get(Restrictions.Eq("Enabled", true));
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
