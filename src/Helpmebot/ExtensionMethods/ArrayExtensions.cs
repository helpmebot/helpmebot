// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArrayExtensions.cs" company="Helpmebot Development Team">
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
//   The string extensions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ExtensionMethods
{
    using System.Linq;

    /// <summary>
    /// The string extensions.
    /// </summary>
    internal static class ArrayExtensions
    {
        /// <summary>
        /// The contains.
        /// </summary>
        /// <typeparam name="T">
        /// The type parameter
        /// </typeparam>
        /// <param name="haystack">
        /// The haystack.
        /// </param>
        /// <param name="needle">
        /// The needle.
        /// </param>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        public static int? PositionOf<T>(this T[] haystack, T needle)
        {
            var id = 0;
            foreach (var straw in haystack)
            {
                if (needle.Equals(straw))
                {
                    return id;
                }

                id++;
            }

            return null;
        }

        /// <summary>
        /// The contains prefix.
        /// </summary>
        /// <param name="haystack">
        /// The haystack.
        /// </param>
        /// <param name="needlehead">
        /// The needle head.
        /// </param>
        /// <returns>
        /// The position, or null.
        /// </returns>
        public static int? ContainsPrefix(this string[] haystack, string needlehead)
        {
            var id = 0;
            foreach (var straw in haystack)
            {
                if (straw.Length >= needlehead.Length)
                {
                    if (needlehead == straw.Substring(0, needlehead.Length))
                    {
                        return id;
                    }
                }

                id++;
            }

            return null;
        }

        /// <summary>
        /// The smart length.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int SmartLength(this string[] data)
        {
            return data.Count(arg => !string.IsNullOrEmpty(arg));
        }
    }
}
