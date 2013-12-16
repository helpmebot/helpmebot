// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallerPriorityAttribute.cs" company="Helpmebot Development Team">
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
//   The installer priority attribute.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Startup
{
    using System;

    /// <summary>
    /// The installer priority attribute.
    /// </summary>
    /// <para>
    /// Use this to specify the order that installers need to be executed in. 
    /// See http://stackoverflow.com/questions/6358206/how-to-force-the-order-of-installer-execution for more information.
    /// </para>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class InstallerPriorityAttribute : Attribute
    {
        /// <summary>
        /// The logger installer priority.
        /// </summary>
        public const int Logger = 1;

        /// <summary>
        /// The database.
        /// </summary>
        public const int Database = 2;
        
        /// <summary>
        /// The default priority assigned whenever this attribute is not applied to a class.
        /// </summary>
        public const int Default = 100;

        /// <summary>
        /// The Windsor setup priority. This is first, and only to be used by the installers which set up parts of Castle Windsor itself.
        /// </summary>
        public const int WindsorSetup = 0;

        /// <summary>
        /// Initialises a new instance of the <see cref="InstallerPriorityAttribute"/> class.
        /// </summary>
        /// <param name="priority">
        /// The priority.
        /// </param>
        public InstallerPriorityAttribute(int priority)
        {
            this.Priority = priority;
        }

        /// <summary>
        /// Gets the priority.
        /// </summary>
        public int Priority { get; private set; }
    }
}
