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
using System.Web;
using System.Xml.XPath;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the edit count of a wikipedian
    /// </summary>
    internal class Editcount : GenericCommand
    {
        /// <summary>
        /// Actual command logic    
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string userName;
            if (args.Length > 0 && args[0] != "")
            {
                userName = string.Join(" ", args);
            }
            else
            {
                userName = source.nickname;
			}
            int editCount = getEditCount(userName, channel);
            if (editCount == -1)
            {
                string[] messageParams = {userName};
                string message = new Message().get("noSuchUser", messageParams);
                return new CommandResponseHandler(message);
            }
            else
            {
                string[] messageParameters = {editCount.ToString(), userName};

                string message = new Message().get("editCount", messageParameters);

                return new CommandResponseHandler(message);
            }
        }

        /// <summary>
        /// Gets the edit count.
        /// </summary>
        /// <param name="username">The username to retrieve the edit count for.</param>
        /// <param name="channel">The channel the command was issued in. (Gets the correct base wiki)</param>
        /// <returns></returns>
        public int getEditCount(string username, string channel)
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

            username = HttpUtility.UrlEncode(username);
   
            XPathDocument xpd =
                new XPathDocument(
                    HttpRequest.get(api + "?format=xml&action=query&list=users&usprop=editcount&format=xml&ususers=" +
                                    username));

            XPathNodeIterator xpni = xpd.CreateNavigator().Select("//user");


            if (xpni.MoveNext())
            {
                string editcount = xpni.Current.GetAttribute("editcount", "");
                if(editcount!= "") return int.Parse(editcount);

                if (xpni.Current.GetAttribute("missing", "") == "")
                    return -1;
            }
            
            throw new ArgumentException();
        }
    }
}