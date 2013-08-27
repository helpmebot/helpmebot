// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rights.cs" company="Helpmebot Development Team">
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
//   Returns the user rights of a wikipedian
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Xml;

    /// <summary>
    ///   Returns the user rights of a wikipedian
    /// </summary>
    internal class Rights : GenericCommand
    {
        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            CommandResponseHandler crh = new CommandResponseHandler();
            
            string userName;
            if (args.Length > 0 && args[0] != "")
            {
                userName = string.Join(" ", args);
            }
            else
            {
                userName = source.nickname;
			}
            string rights = getRights(userName, channel);
            
            string message;
            if (rights != "")
            {
                string[] messageParameters = { userName, rights };
                message = new Message().get("cmdRightsList", messageParameters);
            }
            else
            {
                string[] messageParameters = { userName };
                message = new Message().get("cmdRightsNone", messageParameters);
            }

            crh.respond(message);
            return crh;
        }


        /// <summary>
        /// Gets the rights of a wikipedian.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="channel">The channel to get the base wiki for.</param>
        /// <returns></returns>
        public string getRights(string username, string channel)
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

            string returnStr = "";
            int rightsCount = 0;
            XmlTextReader creader =
                new XmlTextReader(
                    HttpRequest.get(api + "?action=query&list=users&usprop=groups&format=xml&ususers=" + username));
            do
                creader.Read(); while (creader.Name != "user");
            creader.Read();
            if (creader.Name == "groups") //the start of the group list
            {
                do
                {
                    creader.Read();
                    string rightsList = (creader.ReadString());
                    if (!(rightsList == "" || rightsList=="*"))
                        returnStr = returnStr + rightsList + ", ";
                    rightsCount = rightsCount + 1;
                } while (creader.Name == "g"); //each group should be added
            }
            returnStr = rightsCount == 0 ? "" : returnStr.Remove(returnStr.Length - 2);


            return returnStr;
        }
    }
}