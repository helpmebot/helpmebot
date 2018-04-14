// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeferredInstaller.cs" company="Helpmebot Development Team">
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
//   Defines the DeferredInstaller type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Startup.Installers
{
    using Castle.Facilities.Startable;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    /// <summary>
    /// The installer for background services.
    /// </summary>
    public class DeferredInstaller : IWindsorInstaller
    {
        /// <summary>
        /// The install.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="store">
        /// The store.
        /// </param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<StartableFacility>(f => f.DeferredStart());

            container.Register(Classes.FromThisAssembly().InNamespace("Helpmebot.Background").WithService.AllInterfaces());
        }
    }
}
