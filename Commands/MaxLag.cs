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

using System.Reflection;
using System.Xml;

#endregion

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Returns the maximum replication lag on the wiki
    /// </summary>
    internal class Maxlag : GenericCommand
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
            string[] messageParameters = {source.nickname, getMaxLag(channel)};
            string message = new Message().get("cmdMaxLag", messageParameters);
            return new CommandResponseHandler(message);
        }

        /// <summary>
        /// Gets the maximum replication lag between the Wikimedia Foundation MySQL database cluster for the base wiki of the channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        public string getMaxLag(string channel)
        {
            // look up site id
            string baseWiki = Configuration.singleton()["baseWiki",channel];
            // get api

            DAL.Select q = new DAL.Select("site_api");
            q.setFrom("site");
            q.addWhere(new DAL.WhereConds("site_id", baseWiki));
            string api = DAL.singleton().executeScalarSelect(q);

            XmlTextReader mlreader =
                new XmlTextReader(HttpRequest.get(api + "?action=query&meta=siteinfo&siprop=dbrepllag&format=xml"));
            do
            {
                mlreader.Read();
            } while (mlreader.Name != "db");

            string lag = mlreader.GetAttribute("lag");

            return lag;
        }
    }
}