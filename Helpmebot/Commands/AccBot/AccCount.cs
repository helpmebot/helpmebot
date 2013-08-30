// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccCount.cs" company="Helpmebot Development Team">
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
//   Defines the Acccount type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Web;
    using System.Xml.XPath;

    using HttpRequest = helpmebot6.HttpRequest;

    class Acccount : GenericCommand
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

            if (args.Length > 0 && args[0] != "")
            {
                username = string.Join(" ", args);
            }
            else
            {
                username = source.nickname;
            }

            username = HttpUtility.UrlEncode(username);

            XPathDocument xpd =
                new XPathDocument(
                    HttpRequest.get("http://toolserver.org/~acc/api.php?action=count&user=" + username));

            XPathNodeIterator xpni = xpd.CreateNavigator().Select("//user");


            if (xpni.MoveNext())
            {
                if(xpni.Current.GetAttribute("missing", "") == "true")
                {
                    string[] msgparams = {username};
                    string msg = new Message().get("noSuchUser", msgparams);
                    return new CommandResponseHandler(msg);
                }

                string[] adminparams = {
                                            xpni.Current.GetAttribute("suspended", ""), 
                                            xpni.Current.GetAttribute("promoted", ""), 
                                            xpni.Current.GetAttribute("approved", ""), 
                                            xpni.Current.GetAttribute("demoted", ""), 
                                            xpni.Current.GetAttribute("declined", ""), 
                                            xpni.Current.GetAttribute("renamed", ""), 
                                            xpni.Current.GetAttribute("edited", ""), 
                                            xpni.Current.GetAttribute("prefchange", ""), 
                                         };

                string adminmessage = new Message().get("CmdAccCountAdmin", adminparams);

                string[] messageParams = {
                                            username, // username
                                            xpni.Current.GetAttribute("level", ""), // accesslevel
                                            xpni.Current.GetAttribute("created", ""), // numclosed
                                            xpni.Current.GetAttribute("today", ""), // today
                                            xpni.Current.GetAttribute("level", "") == "Admin" ? adminmessage : ""// admin
                                         };

                string message = new Message().get("CmdAccCount", messageParams);
                return new CommandResponseHandler(message);

            }

            throw new ArgumentException();
        }

        #endregion
    }
}
