// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Registration.cs" company="Helpmebot Development Team">
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
//   Returns the registration date of a wikipedian
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Xml;

    /// <summary>
    ///   Returns the registration date of a wikipedian
    /// </summary>
    internal class Registration : GenericCommand
    {
        public Registration(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            CommandResponseHandler crh = new CommandResponseHandler();
            if (args.Length > 0)
            {
                string userName = string.Join(" ", args);
                DateTime registrationDate = getRegistrationDate(userName, channel);
                if (registrationDate == new DateTime(0))
                {
                    string[] messageParams = {userName};
                    string message = new Message().get("noSuchUser", messageParams);
                    crh.respond(message);
                }
                else
                {
                    string[] messageParameters = {
                                                     userName, registrationDate.ToString("hh:mm:ss t"),
                                                     registrationDate.ToString("d MMMM yyyy")
                                                 };
                    string message = new Message().get("registrationDate", messageParameters);
                    crh.respond(message);
                }
            }
            else
            {
                string[] messageParameters = {"registration", "1", args.Length.ToString()};
                Helpmebot6.irc.ircNotice(source.nickname,
                                         new Message().get("notEnoughParameters", messageParameters));
            }
            return crh;
        }

        public static DateTime getRegistrationDate(string username, string channel)
        {
            if (username == string.Empty)
            {
                throw new ArgumentNullException();
            }
            string baseWiki = Configuration.singleton()["baseWiki",channel];

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
        public Reg(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }
    }
}