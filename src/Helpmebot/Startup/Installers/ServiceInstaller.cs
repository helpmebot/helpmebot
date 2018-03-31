// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceInstaller.cs" company="Helpmebot Development Team">
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
//   Defines the ServiceInstaller type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Startup.Installers
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using Helpmebot.Commands;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Services.Geolocation;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Services.UrlShortening;

    /// <summary>
    /// The service installer.
    /// </summary>
    [InstallerPriority(InstallerPriorityAttribute.Default)]
    public class ServiceInstaller : IWindsorInstaller
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
            container.Register(Classes.FromThisAssembly().InNamespace("Helpmebot.Services").WithService.AllInterfaces());
            container.Register(Component.For<ICommandServiceHelper>().ImplementedBy<CommandServiceHelper>());

            container.Register(Component.For<IUrlShorteningService>().ImplementedBy<IsGdUrlShorteningService>());
            container.Register(Component.For<IGeolocationService>().ImplementedBy<MaxMindGeolocationService>());
        }
    }
}
