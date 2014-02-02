﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRepository.cs" company="Helpmebot Development Team">
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
//   Defines the IRepository type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using NHibernate.Criterion;

    /// <summary>
    /// The Repository interface.
    /// </summary>
    /// <typeparam name="T">
    /// The model type
    /// </typeparam>
    public interface IRepository<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        void Save(T model);

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="models">
        /// The models.
        /// </param>
        void Save(IEnumerable<T> models);

        /// <summary>
        /// The get.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{T}"/>.
        /// </returns>
        IEnumerable<T> Get();

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="criterion">
        /// The criterion.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{T}"/>.
        /// </returns>
        IEnumerable<T> Get(ICriterion criterion);

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        void Delete(T model);

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="models">
        /// The models.
        /// </param>
        void Delete(IEnumerable<T> models);

        /// <summary>
        /// The flush.
        /// </summary>
        void Flush();

        /// <summary>
        /// The begin transaction.
        /// </summary>
        /// <param name="level">
        /// The transaction isolation level.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the transaction was started successfully.
        /// </returns>
        bool BeginTransaction(IsolationLevel level = IsolationLevel.Serializable);

        /// <summary>
        /// The roll back.
        /// </summary>
        void RollBack();

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="criterion">
        /// The criterion.
        /// </param>
        void Delete(ICriterion criterion);

        /// <summary>
        /// The commit.
        /// </summary>
        void Commit();

        /// <summary>
        /// The get by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        T GetById(int id);
    }
}