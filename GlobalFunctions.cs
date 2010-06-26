/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/

#region Usings

using System;
using System.Diagnostics;
using System.Reflection;
using MySql.Data.MySqlClient;

#endregion

namespace helpmebot6
{
    public class GlobalFunctions
    {
        /// <summary>
        ///   Searches the array haystack for needle
        /// </summary>
        /// <param name = "needle"></param>
        /// <param name = "haystack"></param>
        /// <returns>ID of the needle in the haystack, -1 if not in array</returns>
        public static int isInArray(string needle, string[] haystack)
        {
            Logger.instance().addToLog(
                string.Format("Method:{0}{1}", MethodBase.GetCurrentMethod().DeclaringType.Name,
                              MethodBase.GetCurrentMethod().Name), Logger.LogTypes.DNWB);

            int id = 0;
            foreach (string straw in haystack)
            {
                if (needle == straw)
                    return id;
                id++;
            }

            return -1;
        }

        /// <summary>
        ///   Searches the array haystack for the first needle who's head matches the needlehead we're looking for
        /// </summary>
        /// <param name = "needlehead"></param>
        /// <param name = "haystack"></param>
        /// <returns>ID of the needle in the haystack, -1 if not in array</returns>
        public static int prefixIsInArray(string needlehead, string[] haystack)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            int id = 0;
            foreach (string straw in haystack)
            {
                if (straw.Length >= needlehead.Length)
                {
                    if (needlehead == straw.Substring(0, needlehead.Length))
                        return id;
                }
                id++;
            }

            return -1;
        }

        /// <summary>
        ///   Remove the first item from an array, and return the item
        /// </summary>
        /// <param name = "list">The array in question</param>
        /// <returns>The first item from the array</returns>
        public static string popFromFront(ref string[] list)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

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
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            Logger.instance().addToLog(ex + ex.StackTrace, Logger.LogTypes.Error);

            StackTrace stack = new StackTrace();
            MethodBase method = stack.GetFrame(1).GetMethod();

            if (Helpmebot6.irc != null)
            {
                Helpmebot6.irc.ircPrivmsg(Helpmebot6.debugChannel, "***ERROR*** in " + method.Name + ": " + ex.Message);
            }
        }

        public static string escape(string str)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            return MySqlHelper.EscapeString(str);
        }

        //public static void Log(string message) { Console.WriteLine("# " + message); }

        public static User.UserRights commandAccessLevel()
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            StackTrace foo = new StackTrace();
            string typename = foo.GetFrame(1).GetMethod().DeclaringType.FullName;

            User.UserRights accessLevel;
            DAL.Select q = new DAL.Select("accesslevel");
            q.setFrom("command");

            q.addWhere(new DAL.WhereConds("typename", typename));
            q.addLimit(1, 0);

            string al = DAL.singleton().executeScalarSelect(q);
            switch (al)
            {
                case "Developer":
                    accessLevel = User.UserRights.Developer;
                    break;
                case "Superuser":
                    accessLevel = User.UserRights.Superuser;
                    break;
                case "Advanced":
                    accessLevel = User.UserRights.Advanced;
                    break;
                case "Normal":
                    accessLevel = User.UserRights.Normal;
                    break;
                case "Semi-ignored":
                    accessLevel = User.UserRights.Semiignored;
                    break;
                case "Ignored":
                    accessLevel = User.UserRights.Ignored;
                    break;
                default:
                    accessLevel = User.UserRights.Developer;
                    errorLog(new ArgumentOutOfRangeException("command", typename, "not found in commandlist"));
                    break;
            }
            return accessLevel;
        }

        public static void removeItemFromArray(string item, ref string[] array)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

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

        public static int realArrayLength(string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            int argsLength = 0;
            foreach (string arg in args)
            {
                if (!string.IsNullOrEmpty(arg))
                    argsLength++;
            }
            return argsLength;
        }

        public static void silentPrivmsg(IAL irc, string channel, string message)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (Configuration.singleton().retrieveLocalStringOption("silence", channel) == "true")
                return;

            irc.ircPrivmsg(channel, message);
        }
    }
}