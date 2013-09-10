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

    /// <summary>
    /// The ACC count.
    /// </summary>
    internal class Acccount : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Acccount"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public Acccount(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] args = this.Arguments;

            string username;

            if (args.Length > 0 && args[0] != string.Empty)
            {
                username = string.Join(" ", args);
            }
            else
            {
                username = this.Source.nickname;
            }

            username = HttpUtility.UrlEncode(username);

            XPathDocument xpd =
                new XPathDocument(
                    HttpRequest.get("http://toolserver.org/~acc/api.php?action=count&user=" + username));

            XPathNodeIterator xpni = xpd.CreateNavigator().Select("//user");

            if (xpni.MoveNext())
            {
                if (xpni.Current.GetAttribute("missing", string.Empty) == "true")
                {
                    string[] msgparams = { username };
                    string msg = new Message().get("noSuchUser", msgparams);
                    return new CommandResponseHandler(msg);
                }

                string[] adminparams =
                    {
                        xpni.Current.GetAttribute("suspended", string.Empty),
                        xpni.Current.GetAttribute("promoted", string.Empty),
                        xpni.Current.GetAttribute("approved", string.Empty),
                        xpni.Current.GetAttribute("demoted", string.Empty),
                        xpni.Current.GetAttribute("declined", string.Empty),
                        xpni.Current.GetAttribute("renamed", string.Empty),
                        xpni.Current.GetAttribute("edited", string.Empty),
                        xpni.Current.GetAttribute("prefchange", string.Empty)
                    };

                string adminmessage = new Message().get("CmdAccCountAdmin", adminparams);

                string[] messageParams =
                    {
                        username, // username
                        xpni.Current.GetAttribute("level", string.Empty), // accesslevel
                        xpni.Current.GetAttribute("created", string.Empty), // numclosed
                        xpni.Current.GetAttribute("today", string.Empty), // today
                        xpni.Current.GetAttribute("level", string.Empty) == "Admin"
                            ? adminmessage
                            : string.Empty // admin
                    };

                string message = new Message().get("CmdAccCount", messageParams);
                return new CommandResponseHandler(message);
            }

            throw new ArgumentException();
        }

        #endregion
    }
}
