// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateExtensions.cs" company="Helpmebot Development Team">
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
//   Defines the DateExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ExtensionMethods
{
    using System;

    /// <summary>
    /// The date extensions.
    /// </summary>
    public static class DateExtensions
    {
        /// <summary>
        /// Converts a date to internet timestamp (TZ) format.
        /// </summary>
        /// <param name="date">
        /// The date.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToInternetFormat(this DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}
