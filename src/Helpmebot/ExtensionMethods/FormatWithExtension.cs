// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FormatWithExtension.cs" company="Helpmebot Development Team">
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
//   Defines the FormatWithExtension type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The format with extension.
    /// </summary>
    public static class FormatWithExtension
    {
        /// <summary>
        /// The format with.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the format is null.
        /// </exception>
        public static string FormatWith(this string format, IDictionary<string,object> source)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }

            return source.Aggregate(format, (current, keyValuePair) => current.Replace("{" + keyValuePair.Key + "}", keyValuePair.Value.ToString()));
        }
    }
}
