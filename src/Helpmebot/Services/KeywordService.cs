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
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Commands.Brain;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;

    public class KeywordService : IKeywordService
    {
        private readonly ILogger logger;
        private readonly ICommandParser commandParser;

        private readonly ISession session;
        private readonly object sessionLock = new object();

        private readonly HashSet<string> registeredCommands = new HashSet<string>();

        public KeywordService(ILogger logger, ICommandParser commandParser, ISession session)
        {
            this.logger = logger;
            this.commandParser = commandParser;
            this.session = session;
        }

        /// <summary>
        /// Deletes a learned word
        /// </summary>
        /// <param name="name">
        /// The keyword to delete.
        /// </param>
        public void Delete(string name)
        {
            lock (this.sessionLock)
            {
                var deleteList = this.session.CreateCriteria<Keyword>()
                    .Add(Restrictions.Eq("Name", name))
                    .List<Keyword>();

                foreach (var model in deleteList)
                {
                    this.logger.DebugFormat("Deleting model {0} ({1})...", model, model.GetType().Name);
                    this.session.Delete(model);
                    this.UnregisterCommand(name);
                }

                this.session.Flush();
            }
        }

        /// <summary>
        /// Creates a new learned word
        /// </summary>
        /// <param name="name">
        /// The keyword to store and retrieve with
        /// </param>
        /// <param name="response">
        /// The response to give
        /// </param>
        /// <param name="action">
        /// Flag indicating if this response should be given as a CTCP ACTION
        /// </param>
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

                    this.logger.Debug("Transactional create function succeeded.");
                    transaction.Commit();
                    
                    this.RegisterCommand(name);
                }
                catch (Exception ex)
                {
                    this.logger.Error("Transactional create function failed", ex);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Retrieves a stored keyword
        /// </summary>
        /// <param name="name">
        /// The keyword to retrieve.
        /// </param>
        /// <returns>
        /// An object representing the keyword
        /// </returns>
        public Keyword Get(string name)
        {
            IList<Keyword> existing;
            lock (this.sessionLock)
            {
                existing = this.session.CreateCriteria<Keyword>().Add(Restrictions.Eq("Name", name)).List<Keyword>();
            }

            return existing.FirstOrDefault();
        }

        public void Start()
        {
            IList<Keyword> keywords;
            lock (this.sessionLock)
            {
                keywords = this.session.CreateCriteria<Keyword>().List<Keyword>();
            }

            this.logger.InfoFormat("Populating command parser with {0} stored responses", keywords.Count);

            foreach (var keyword in keywords)
            {
                this.RegisterCommand(keyword.Name);
            }
            
            lock (this.registeredCommands)
            {
                this.logger.InfoFormat("Registered {0} stored responses in command parser", this.registeredCommands.Count);
            }
        }
        
        public void Stop()
        {
            HashSet<string> set;
            lock (this.registeredCommands)
            {
                set = new HashSet<string>(this.registeredCommands);
                this.logger.InfoFormat(
                    "Shutting down keyword service with {0} registered commands",
                    this.registeredCommands.Count);
            }

            foreach (var command in set)
            {
                this.UnregisterCommand(command);
            }
        }

        private void RegisterCommand(string keywordName)
        {
            if (!this.UnregisterCommand(keywordName))
            {
                return;
            }

            this.commandParser.RegisterCommand(keywordName, typeof(BrainRetrievalCommand));
            
            lock (this.registeredCommands)
            {
                this.registeredCommands.Add(keywordName);
            }

            this.logger.DebugFormat("Registered keyword {0}.", keywordName);
        }

        private bool UnregisterCommand(string keywordName)
        {
            var existingCommand = this.commandParser.GetRegisteredCommand(keywordName);
            if (existingCommand != null)
            {
                if (existingCommand != typeof(BrainRetrievalCommand))
                {
                    this.logger.WarnFormat(
                        "Could not unregister keyword {0} with command parser as this command is not a keyword command.",
                        keywordName);
                    return false;
                }

                this.logger.DebugFormat("Unregistered keyword {0}.", keywordName);

                lock (this.registeredCommands)
                {
                    this.registeredCommands.Remove(keywordName);
                }

                this.commandParser.UnregisterCommand(keywordName);
            }

            return true;
        }

    }
}