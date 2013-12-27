// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindsorBootstrap.cs" company="Helpmebot Development Team">
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
//   Defines the WindsorBootstrap type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Startup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Windsor.Installer;

    /// <summary>
    /// Bootstraps Windsor's installer to deal with the priority of installers.
    /// </summary>
    public class WindsorBootstrap : InstallerFactory
    {
        /// <summary>
        /// The select.
        /// </summary>
        /// <param name="installerTypes">
        /// The installer types.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{Type}"/>.
        /// </returns>
        public override IEnumerable<Type> Select(IEnumerable<Type> installerTypes)
        {
            IOrderedEnumerable<Type> orderedInstallers = installerTypes.OrderBy(this.GetPriority);
            var enumerable = orderedInstallers.Where(
                x => !x.GetCustomAttributes(typeof(DeferredInstallerAttribute), false).Any());
            return enumerable;
        }

        /// <summary>
        /// The get priority.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        protected int GetPriority(Type type)
        {
            var attribute =
                type.GetCustomAttributes(typeof(InstallerPriorityAttribute), false).FirstOrDefault() as
                InstallerPriorityAttribute;
            return attribute != null ? attribute.Priority : InstallerPriorityAttribute.Default;
        }
    }
}
