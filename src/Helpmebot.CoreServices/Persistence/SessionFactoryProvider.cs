// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionFactoryProvider.cs" company="Helpmebot Development Team">
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
//   Defines the SessionFactoryProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Persistence
{
    using System;

    using Helpmebot.Persistence.Interfaces;

    using NHibernate;
    using NHibernate.Cfg;

    /// <summary>
    /// The session factory provider.
    /// </summary>
    public class SessionFactoryProvider : ISessionFactoryProvider
    {
        /// <summary>
        /// The config.
        /// </summary>
        private readonly Configuration config;

        /// <summary>
        /// Initialises a new instance of the <see cref="SessionFactoryProvider"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        public SessionFactoryProvider(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Gets the session factory.
        /// </summary>
        public virtual ISessionFactory SessionFactory { get; private set; }

        /// <summary>
        /// The initialize.
        /// </summary>
        public virtual void Initialize()
        {
            this.SessionFactory = this.config.BuildSessionFactory();
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
                this.SessionFactory.Close();
            }
        }
    }
}