// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupRepository.cs" company="Helpmebot Development Team">
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
//   Defines the FlagGroupRepository type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories
{
    using Castle.Core.Logging;

    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using NHibernate;

    /// <summary>
    /// The flag group repository.
    /// </summary>
    public class FlagGroupRepository : RepositoryBase<FlagGroup>, IFlagGroupRepository
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="FlagGroupRepository"/> class.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public FlagGroupRepository(ISession session, ILogger logger)
            : base(session, logger)
        {
        }
    }
}
