// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindsorServiceLocator.cs" company="Helpmebot Development Team">
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
//   Defines the WindsorServiceLocator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;
    using System.Collections.Generic;

    using Castle.Windsor;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// The windsor service locator.
    /// </summary>
    internal class WindsorServiceLocator : ServiceLocatorImplBase
    {
        /// <summary>
        /// The container.
        /// </summary>
        private readonly IWindsorContainer container;

        /// <summary>
        /// Initialises a new instance of the <see cref="WindsorServiceLocator"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public WindsorServiceLocator(IWindsorContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// The do get instance.
        /// </summary>
        /// <param name="serviceType">
        /// The service type.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (key != null)
            {
                return this.container.Resolve(key, serviceType);
            }

            return this.container.Resolve(serviceType);
        }

        /// <summary>
        /// The do get all instances.
        /// </summary>
        /// <param name="serviceType">
        /// The service type.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Object}"/>.
        /// </returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return (object[])this.container.ResolveAll(serviceType);
        }
    }
}
