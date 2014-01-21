// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RepositoryBase.cs" company="Helpmebot Development Team">
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
//   The repository base.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Castle.Core.Logging;

    using Helpmebot.Repositories.Interfaces;

    using NHibernate;
    using NHibernate.Criterion;
    using NHibernate.Linq;

    /// <summary>
    /// The repository base.
    /// </summary>
    /// <typeparam name="T">
    /// The model
    /// </typeparam>
    public abstract class RepositoryBase<T> : IRepository<T>
        where T : class
    {
        /// <summary>
        /// The session.
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initialises a new instance of the <see cref="RepositoryBase{T}"/> class. 
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        protected RepositoryBase(ISession session, ILogger logger)
        {
            this.session = session;
            this.Logger = logger;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the session.
        /// </summary>
        protected ISession Session
        {
            get
            {
                return this.session;
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public void Save(T model)
        {
            this.DoSave(model);
            this.Flush();
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="models">
        /// The models.
        /// </param>
        public void Save(IEnumerable<T> models)
        {
            models.ForEach(this.DoSave);
            this.Flush();
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{WelcomeUser}"/>.
        /// </returns>
        public IEnumerable<T> Get()
        {
            return this.Session.CreateCriteria<T>().List<T>();
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="criterion">
        /// The criterion.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{WelcomeUser}"/>.
        /// </returns>
        public IEnumerable<T> Get(ICriterion criterion)
        {
            return this.Session.CreateCriteria<T>().Add(criterion).List<T>();
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public void Delete(T model)
        {
            this.DoDelete(model);
            this.Flush();
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="models">
        /// The models.
        /// </param>
        public void Delete(IEnumerable<T> models)
        {
            models.ForEach(this.Delete);
            this.Flush();
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="criterion">
        /// The criterion.
        /// </param>
        public void Delete(ICriterion criterion)
        {
            var deleteList = this.session.CreateCriteria<T>().Add(criterion).List<T>();
            this.Delete(deleteList);
        }

        /// <summary>
        /// The flush.
        /// </summary>
        public void Flush()
        {
            this.session.Flush();
        }

        /// <summary>
        /// The begin transaction.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the transaction was started successfully.
        /// </returns>
        public virtual bool BeginTransaction()
        {
            this.session.BeginTransaction(IsolationLevel.Serializable);

            // FIXME: !!
            return true;
        }

        /// <summary>
        /// The roll back.
        /// </summary>
        public virtual void RollBack()
        {
            this.session.Transaction.Rollback();
        }

        /// <summary>
        /// The commit.
        /// </summary>
        public virtual void Commit()
        {
            this.session.Transaction.Commit();
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Flush();
                this.session.Close();
                this.session.Dispose();
            }
        }

        /// <summary>
        /// The do save.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        protected virtual void DoSave(T model)
        {
            this.Logger.DebugFormat("Saving model {0} ({1})...", model, model.GetType().Name);
            this.Session.SaveOrUpdate(model);
        }        
        
        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        protected virtual void DoDelete(T model)
        {
            this.Logger.DebugFormat("Deleting model {0} ({1})...", model, model.GetType().Name);
            this.Session.Delete(model);
        }
    }
}