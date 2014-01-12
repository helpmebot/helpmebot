// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="Helpmebot Development Team">
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
//   Defines the EnumerableExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The enumerable extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// The implode.
        /// </summary>
        /// <param name="value">
        /// The list.
        /// </param>
        /// <param name="separator">
        /// The separator.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string Implode(this IEnumerable<string> value, string separator = " ")
        {
            return string.Join(separator, value.ToArray());
        }

        /// <summary>
        /// The apply.
        /// </summary>
        /// <typeparam name="T">
        /// The type of enumerable
        /// </typeparam>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        public static void Apply<T>(this IEnumerable<T> value, Action<T> action)
        {
            foreach (var x in value)
            {
                action(x);
            }
        }
    }
}
