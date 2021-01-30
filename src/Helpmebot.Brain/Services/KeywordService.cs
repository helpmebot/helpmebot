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

namespace Helpmebot.Brain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Brain.Commands;
    using Helpmebot.Brain.Services.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;

    public class KeywordService : CommandParserProviderServiceBase<Keyword>, IKeywordService
    {
        private readonly ISession session;
        private readonly object sessionLock = new object();

        public KeywordService(ILogger logger, ICommandParser commandParser, ISession session)
            : base(commandParser, logger)
        {
            this.session = session;
        }

        /// <inheritdoc />
        public void Delete(string name)
        {
            lock (this.sessionLock)
            {
                using (var txn = this.session.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var deleteList = this.session.CreateCriteria<Keyword>()
                        .Add(Restrictions.Eq("Name", name))
                        .List<Keyword>();

                    foreach (var model in deleteList)
                    {
                        this.Logger.DebugFormat("Deleting model {0} ({1})...", model, model.GetType().Name);
                        this.session.Delete(model);
                        this.UnregisterCommand(model);
                    }

                    txn.Commit();
                }
            }
        }

        /// <inheritdoc />
        public void Create(string name, string response, bool action)
        {
            lock (this.sessionLock)
            {
                var transaction = this.session.BeginTransaction(IsolationLevel.Serializable);

                if (!transaction.IsActive)
                {
                    throw new TransactionException("Could not start transaction!");
                }

                try
                {
                    var existing =
                        this.session.CreateCriteria<Keyword>()
                            .Add(Restrictions.Eq("Name", name))
                            .List<Keyword>()
                            .FirstOrDefault() ?? new Keyword();

                    existing.Name = name;
                    existing.Response = response;
                    existing.Action = action;

                    this.session.SaveOrUpdate(existing);

                    this.Logger.Debug("Transactional create function succeeded.");
                    transaction.Commit();

                    this.RegisterCommand(existing);
                }
                catch (Exception ex)
                {
                    this.Logger.Error("Transactional create function failed", ex);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public Keyword Get(string name)
        {
            IList<Keyword> existing;
            lock (this.sessionLock)
            {
                existing = this.session.CreateCriteria<Keyword>().Add(Restrictions.Eq("Name", name)).List<Keyword>();
            }

            return existing.FirstOrDefault();
        }

        protected override IList<Keyword> ItemsToRegister()
        {
            IList<Keyword> keywords;
            lock (this.sessionLock)
            {
                keywords = this.session.CreateCriteria<Keyword>().List<Keyword>();
            }

            return keywords;
        }

        protected override Type CommandImplementation()
        {
            return typeof(BrainRetrievalCommand);
        }
    }
}