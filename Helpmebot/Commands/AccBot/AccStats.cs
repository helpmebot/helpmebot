// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccStats.cs" company="Helpmebot Development Team">
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
//   Defines the Accstats type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Web;
    using System.Xml.XPath;

    using HttpRequest = helpmebot6.HttpRequest;

    internal class Accstats : GenericCommand
    {
        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            string username;

            if (args.Length > 0 && args[0] != string.Empty)
            {
                username = string.Join(" ", args);
            }
            else
            {
                username = source.nickname;
            }

            username = HttpUtility.UrlEncode(username);

            XPathDocument xpd =
                new XPathDocument(HttpRequest.get("http://toolserver.org/~acc/api.php?action=stats&user=" + username));

            XPathNodeIterator xpni = xpd.CreateNavigator().Select("//user");


            if (xpni.MoveNext())
            {
                if (xpni.Current.GetAttribute("missing", string.Empty) == "true")
                {
                    string[] msgparams = { username };
                    string msg = new Message().get("noSuchUser", msgparams);
                    return new CommandResponseHandler(msg);
                }

                string[] messageParams =
                    {
                        username, // username
                        xpni.Current.GetAttribute("user_level", string.Empty), // accesslevel
                        xpni.Current.GetAttribute("user_lastactive", string.Empty),
                        xpni.Current.GetAttribute("user_welcome_templateid", string.Empty) == "0"
                            ? "disabled"
                            : "enabled",
                        xpni.Current.GetAttribute("user_onwikiname", string.Empty)
                    };

                string message = new Message().get("CmdAccStats", messageParams);
                return new CommandResponseHandler(message);

            }

            throw new ArgumentException();
        }

        #endregion
    }
}
