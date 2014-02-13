// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationInstaller.cs" company="Helpmebot Development Team">
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
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Startup.Installers
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using Helpmebot.Configuration;
    using Helpmebot.Configuration.XmlSections.Interfaces;

    /// <summary>
    ///     The configuration installer.
    /// </summary>
    [InstallerPriority(InstallerPriorityAttribute.Configuration)]
    public class ConfigurationInstaller : IWindsorInstaller
    {
        #region Public Methods and Operators

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
            container.Register(Component.For<IConfigurationHelper>().ImplementedBy<ConfigurationHelper>());
            var configurationHelper = container.Resolve<IConfigurationHelper>();

            container.Register(Component.For<ICoreConfiguration>().Instance(configurationHelper.CoreConfiguration));
            container.Register(Component.For<IPrivateConfiguration>().Instance(configurationHelper.PrivateConfiguration));
            container.Register(Component.For<IIrcConfiguration>().Instance(configurationHelper.IrcConfiguration));
        }

        #endregion
    }
}