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
        /// The session lock.
        /// </summary>
        private readonly object sessionLock = new object();

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
            IList<T> list;

            lock (this.sessionLock)
            {
                list = this.session.CreateCriteria<T>().List<T>();
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
            lock (this.sessionLock)
            {
                this.session.Flush();
            }
        }

        /// <summary>
        /// The begin transaction.
        /// </summary>
        /// <param name="level">
        /// The transaction isolation level.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the transaction was started successfully.
        /// </returns>
        public virtual bool BeginTransaction(IsolationLevel level = IsolationLevel.Serializable)
        {
            lock (this.sessionLock)
            {
                var transaction = this.session.BeginTransaction(level);

                if (!transaction.IsActive)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// The roll back.
        /// </summary>
        public virtual void RollBack()
        {
            lock (this.sessionLock)
            {
                if (this.session.Transaction.IsActive)
                {
                    this.session.Transaction.Rollback();
                }
                else
                {
                    this.Logger.Error("Can't rollback non-existing transaction!");
                    throw new TransactionException("Can't rollback non-existing transaction!");
                }
            }
        }

        /// <summary>
        /// The commit.
        /// </summary>
        public virtual void Commit()
        {
            if (this.session.Transaction.IsActive)
            {
                this.session.Transaction.Commit();
            }
            else
            {
                this.Logger.Warn("Skipped committing non-existing transaction!");
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
            lock (this.sessionLock)
            {
                this.Logger.DebugFormat("Saving model {0} ({1})...", model, model.GetType().Name);
                this.session.SaveOrUpdate(model);
            }
        }        
        
        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        protected virtual void DoDelete(T model)
        {
            lock (this.sessionLock)
            {
                this.Logger.DebugFormat("Deleting model {0} ({1})...", model, model.GetType().Name);
                this.session.Delete(model);
            }
        }
    }
}