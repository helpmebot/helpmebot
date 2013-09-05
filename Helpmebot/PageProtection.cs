// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PageProtection.cs" company="Helpmebot Development Team">
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
//   Structure to hold page protection information
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6
{
    using System;

    /// <summary>
    /// Structure to hold page protection information
    /// </summary>
    internal struct PageProtection
    {
        /// <summary>
        /// The type.
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// The level.
        /// </summary>
        public readonly string Level;

        /// <summary>
        /// The expiry.
        /// </summary>
        public DateTime Expiry;

        /// <summary>
        /// Initialises a new instance of the <see cref="PageProtection"/> struct.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="level">
        /// The level.
        /// </param>
        /// <param name="expiry">
        /// The expiry.
        /// </param>
        public PageProtection(string type, string level, DateTime expiry)
        {
            this.Type = type;
            this.Level = level;
            this.Expiry = expiry;
        }
    }
}