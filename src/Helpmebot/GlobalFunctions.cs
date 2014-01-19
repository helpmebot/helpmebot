// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalFunctions.cs" company="Helpmebot Development Team">
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
namespace Helpmebot
{
    using System.Linq;

    /// <summary>
    ///     Class holding globally accessible functions
    /// </summary>
    public class GlobalFunctions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Remove the first item from an array, and return the item
        /// </summary>
        /// <param name="list">
        /// The array in question
        /// </param>
        /// <returns>
        /// The first item from the array
        /// </returns>
        public static string PopFromFront(ref string[] list)
        {
            string firstItem = list[0];
            list = string.Join(" ", list, 1, list.Length - 1).Split(' ');
            return firstItem;
        }

        /// <summary>
        /// The remove item from array.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="array">
        /// The array.
        /// </param>
        public static void RemoveItemFromArray(string item, ref string[] array)
        {
            var count = array.Count(i => i == item);

            var newArray = new string[array.Length - count];

            var nextAddition = 0;

            foreach (var i in array.Where(i => i != item))
            {
                newArray[nextAddition] = i;
                nextAddition++;
            }

            array = newArray;
        }

        #endregion
    }
}