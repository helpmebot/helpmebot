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
    using System;
    using System.Linq;

    using Castle.Core.Logging;

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
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="KeywordService"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public KeywordService(IKeywordRepository repository, ILogger logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public void Delete(string name)
        {
            if (!this.repository.BeginTransaction())
            {
                this.logger.Warn("Transaction failed to start!");
                return;
            }

            try
            {
                this.repository.Delete(Restrictions.Eq("Name", name));
                this.repository.Commit();
            }
            catch (Exception ex)
            {
                this.logger.Error("Error in transaction.", ex);
                this.repository.RollBack();
            }
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
            if (!this.repository.BeginTransaction())
            {
                this.logger.Warn("Transaction failed to start!");
                return;
            }

            try
            {
                Keyword existing = this.repository.GetByName(name).Any()
                                       ? this.repository.GetByName(name).First()
                                       : new Keyword { Name = name };

                existing.Action = action;
                existing.Response = response;

                this.repository.Save(existing);
                this.repository.Commit();
            }
            catch (Exception ex)
            {
                this.logger.Error("Error in transaction.", ex);
                this.repository.RollBack();
            }
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
