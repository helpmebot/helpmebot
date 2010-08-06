// /****************************************************************************
//  *   This file is part of Helpmebot.                                        *
//  *                                                                          *
//  *   Helpmebot is free software: you can redistribute it and/or modify      *
//  *   it under the terms of the GNU General Public License as published by   *
//  *   the Free Software Foundation, either version 3 of the License, or      *
//  *   (at your option) any later version.                                    *
//  *                                                                          *
//  *   Helpmebot is distributed in the hope that it will be useful,           *
//  *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
//  *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
//  *   GNU General Public License for more details.                           *
//  *                                                                          *
//  *   You should have received a copy of the GNU General Public License      *
//  *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
//  ****************************************************************************/
#region Usings

using System;
using System.Reflection;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the age of a wikipedian
    /// </summary>
    internal class Age : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (args.Length > 0)
            {
                string username = string.Join(" ", args);
                TimeSpan time = getWikipedianAge(username, channel);
                string message;
                if (time.Equals(new TimeSpan(0)))
                {
                    string[] messageParameters = {username};
                    message = Configuration.singleton().getMessage("noSuchUser", messageParameters);
                }
                else
                {
                    string[] messageParameters = {
                                                     username, (time.Days/365).ToString(), (time.Days%365).ToString(),
                                                     time.Hours.ToString(), time.Minutes.ToString(),
                                                     time.Seconds.ToString()
                                                 };
                    message = Configuration.singleton().getMessage("cmdAge", messageParameters);
                }
                return new CommandResponseHandler(message);
            }
            string[] messageParameters2 = {"age", "1", args.Length.ToString()};
            Helpmebot6.irc.ircNotice(source.nickname,
                                     Configuration.singleton().getMessage("notEnoughParameters", messageParameters2));
            return null;
        }

        public TimeSpan getWikipedianAge(string userName, string channel)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            Registration regCommand = new Registration();
            DateTime regdate = regCommand.getRegistrationDate(userName, channel);
            TimeSpan age = DateTime.Now.Subtract(regdate);
            if (regdate.Equals(new DateTime(0001, 1, 1)))
            {
                age = new TimeSpan(0);
            }
            return age;
        }
    }
}