// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppSettingsDependencyResolver.cs" company="Helpmebot Development Team">
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
//   The app settings dependency resolver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Startup.Resolvers
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Linq;

    using Castle.Core;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Context;

    /// <summary>
    /// The app settings dependency resolver.
    /// </summary>
    public class AppSettingsDependencyResolver : ISubDependencyResolver
    {
        #region Public Methods and Operators

        /// <summary>
        /// Returns true if the resolver is able to satisfy this dependency.
        /// </summary>
        /// <param name="context">
        /// Creation context, which is a resolver itself
        /// </param>
        /// <param name="contextHandlerResolver">
        /// Parent resolver - normally the IHandler implementation
        /// </param>
        /// <param name="model">
        /// Model of the component that is requesting the dependency
        /// </param>
        /// <param name="dependency">
        /// The dependency model
        /// </param>
        /// <returns>
        /// <c>true</c> if the dependency can be satisfied
        /// </returns>
        public bool CanResolve(
            CreationContext context, 
            ISubDependencyResolver contextHandlerResolver, 
            ComponentModel model, 
            DependencyModel dependency)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(dependency.DependencyKey)
                   && TypeDescriptor.GetConverter(dependency.TargetType).CanConvertFrom(typeof(string));
        }

        /// <summary>
        /// Should return an instance of a service or property values as
        ///               specified by the dependency model instance. 
        ///               It is also the responsibility of <see cref="T:Castle.MicroKernel.IDependencyResolver"/>
        ///               to throw an exception in the case a non-optional dependency 
        ///               could not be resolved.
        /// </summary>
        /// <param name="context">
        /// Creation context, which is a resolver itself
        /// </param>
        /// <param name="contextHandlerResolver">
        /// Parent resolver - normally the IHandler implementation
        /// </param>
        /// <param name="model">
        /// Model of the component that is requesting the dependency
        /// </param>
        /// <param name="dependency">
        /// The dependency model
        /// </param>
        /// <returns>
        /// The dependency resolved value or null
        /// </returns>
        public object Resolve(
            CreationContext context, 
            ISubDependencyResolver contextHandlerResolver, 
            ComponentModel model, 
            DependencyModel dependency)
        {
            return
                TypeDescriptor.GetConverter(dependency.TargetType)
                    .ConvertFrom(ConfigurationManager.AppSettings[dependency.DependencyKey]);
        }

        #endregion
    }
}