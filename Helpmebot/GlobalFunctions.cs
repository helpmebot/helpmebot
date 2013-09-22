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
// <summary>
//   Class holding globally accessible functions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    using Helpmebot;

    using helpmebot6.ExtensionMethods;

    /// <summary>
    /// Class holding globally accessible functions
    /// </summary>
    public class GlobalFunctions
    {
        /// <summary>
        ///   Remove the first item from an array, and return the item
        /// </summary>
        /// <param name = "list">The array in question</param>
        /// <returns>The first item from the array</returns>
        public static string popFromFront(ref string[] list)
        {
            string firstItem = list[0];
            list = string.Join(" ", list, 1, list.Length - 1).Split(' ');
            return firstItem;
        }

        /// <summary>
        ///   Log an exception to the log and IRC
        /// </summary>
        /// <param name = "ex">The exception thrown</param>
        public static void errorLog(Exception ex)
        {
            Logger.instance().addToLog(ex + ex.StackTrace, Logger.LogTypes.Error);

            StackTrace stack = new StackTrace();
            MethodBase method = stack.GetFrame(1).GetMethod();

            if (Helpmebot6.irc != null)
            {
                Helpmebot6.irc.ircPrivmsg(Helpmebot6.debugChannel, "***ERROR*** in " + method.Name + ": " + ex.Message);
            }
        }



        public static void removeItemFromArray(string item, ref string[] array)
        {
            int count = 0;
            foreach (string i in array)
            {
                if (i == item)
                    count++;
            }

            string[] newArray = new string[array.Length - count];

            int nextAddition = 0;

            foreach (string  i in array)
            {
                if (i == item) continue;
                newArray[nextAddition] = i;
                nextAddition++;
            }

            array = newArray;
        }
    }
}