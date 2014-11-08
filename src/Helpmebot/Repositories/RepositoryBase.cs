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
        #region Fields

        /// <summary>
        /// The session.
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The session lock.
        /// </summary>
        private readonly object sessionLock = new object();

        #endregion

        #region Constructors and Destructors

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public void Delete(T model)
        {
            lock (this.sessionLock)
            {
                this.Logger.DebugFormat("Deleting model {0} ({1})...", model, model.GetType().Name);
                this.session.Delete(model);
                this.session.Flush();
            }
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="models">
        /// The models.
        /// </param>
        public void Delete(IEnumerable<T> models)
        {
            lock (this.sessionLock)
            {
                this.DoDelete(models);
                this.session.Flush();
            }
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="criterion">
        /// The criterion.
        /// </param>
        public void Delete(ICriterion criterion)
        {
            lock (this.sessionLock)
            {
                var deleteList = this.session.CreateCriteria<T>().Add(criterion).List<T>();
                this.DoDelete(deleteList);

                this.session.Flush();
            }
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
        /// The get.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{WelcomeUser}"/>.
        /// </returns>
        public IEnumerable<T> Get()
        {
            IList<T> list;

            lock (this.sessionLock)
            {
                list = this.session.CreateCriteria<T>().List<T>();
            }

            return list;
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
            IList<T> list;
            lock (this.sessionLock)
            {
                list = this.session.CreateCriteria<T>().Add(criterion).List<T>();
            }

            return list;
        }

        /// <summary>
        /// The get by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public T GetById(int id)
        {
            T uniqueResult;
            lock (this.sessionLock)
            {
                uniqueResult = this.session.CreateCriteria<T>().Add(Restrictions.Eq("Id", id)).UniqueResult<T>();
            }

            return uniqueResult;
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        public void Save(T model)
        {
            lock (this.sessionLock)
            {
                this.DoSave(model);
                this.session.Flush();
            }
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="models">
        /// The models.
        /// </param>
        public void Save(IEnumerable<T> models)
        {
            lock (this.sessionLock)
            {
                models.ForEach(this.DoSave);
                this.session.Flush();
            }
        }

        /// <summary>
        /// Executes a callback in the context of a transaction.
        /// </summary>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        public void Transactionally(Action<ISession> callback, IsolationLevel level = IsolationLevel.Serializable)
        {
            lock (this.sessionLock)
            {
                var transaction = this.session.BeginTransaction(level);

                if (!transaction.IsActive)
                {
                    throw new TransactionException("Could not start transaction!");
                }

                try
                {
                    callback(this.session);

                    this.Logger.Debug("Transactional function succeeded.");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    this.Logger.Error("Transactional function failed", ex);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region Methods

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
                lock (this.sessionLock)
                {
                    this.session.Flush();
                    this.session.Close();
                    this.session.Dispose();
                }
            }
        }

        /// <summary>
        /// Deletes a list of models
        /// </summary>
        /// <param name="deleteList">
        /// The delete list.
        /// </param>
        private void DoDelete(IEnumerable<T> deleteList)
        {
            foreach (var model in deleteList)
            {
                this.DoDelete(model);
            }
        }

        /// <summary>
        /// Actually deletes a model
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        private void DoDelete(T model)
        {
            this.Logger.DebugFormat("Deleting model {0} ({1})...", model, model.GetType().Name);
            this.session.Delete(model);
        }

        /// <summary>
        /// Perform the actual save
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        private void DoSave(T model)
        {
            this.Logger.DebugFormat("Saving model {0} ({1})...", model, model.GetType().Name);
            this.session.SaveOrUpdate(model);
        }

        #endregion
    }
}