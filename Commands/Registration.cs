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
using System.Xml;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the registration date of a wikipedian
    /// </summary>
    internal class Registration : GenericCommand
    {
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            CommandResponseHandler crh = new CommandResponseHandler();
            if (args.Length > 0)
            {
                string userName = string.Join(" ", args);
                DateTime registrationDate = getRegistrationDate(userName, channel);
                if (registrationDate == new DateTime(0))
                {
                    string[] messageParams = {userName};
                    string message = Configuration.singleton().getMessage("noSuchUser", messageParams);
                    crh.respond(message);
                }
                else
                {
                    string[] messageParameters = {
                                                     userName, registrationDate.ToString("hh:mm:ss t"),
                                                     registrationDate.ToString("d MMMM yyyy")
                                                 };
                    string message = Configuration.singleton().getMessage("registrationDate", messageParameters);
                    crh.respond(message);
                }
            }
            else
            {
                string[] messageParameters = {"registration", "1", args.Length.ToString()};
                Helpmebot6.irc.ircNotice(source.nickname,
                                         Configuration.singleton().getMessage("notEnoughParameters", messageParameters));
            }
            return crh;
        }

        public DateTime getRegistrationDate(string username, string channel)
        {
            if (username == string.Empty)
            {
                throw new ArgumentNullException();
            }
            string baseWiki = Configuration.singleton().retrieveLocalStringOption("baseWiki", channel);

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);
            XmlTextReader creader =
                new XmlTextReader(
                    HttpRequest.get(api + "?action=query&list=users&usprop=registration&format=xml&ususers=" + username));
            do
            {
                creader.Read();
            } while (creader.Name != "user");
            string apiRegDate = creader.GetAttribute("registration");
            if (apiRegDate != null)
            {
                if (apiRegDate == "")
                {
                    return new DateTime(1970, 1, 1, 0, 0, 0);
                }
                DateTime regDate = DateTime.Parse(apiRegDate);
                return regDate;
            }
            return new DateTime(0);
        }
    }

    /// <summary>
    ///   Returns the registration date of a wikipedian. Alias for Registration
    /// </summary>
    internal class Reg : Registration
    {
    }
}