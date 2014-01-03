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

    using Helpmebot;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    using HttpRequest = Helpmebot.HttpRequest;

    /// <summary>
    /// The stats of the account creation interface.
    /// </summary>
    internal class Accstats : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Accstats"/> class.
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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Accstats(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string username;

            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                username = string.Join(" ", this.Arguments);
            }
            else
            {
                username = this.Source.Nickname;
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
                    string msg = this.MessageService.RetrieveMessage("noSuchUser", this.Channel, msgparams);
                    return new CommandResponseHandler(msg);
                }

                string[] messageParams =
                    {
                        username, // username
                        xpni.Current.GetAttribute("status", string.Empty), // accesslevel
                        xpni.Current.GetAttribute("lastactive", string.Empty),
                        xpni.Current.GetAttribute("welcome_template", string.Empty) == "0"
                            ? "disabled"
                            : "enabled",
                        xpni.Current.GetAttribute("onwikiname", string.Empty)
                    };

                string message = this.MessageService.RetrieveMessage("CmdAccStats", this.Channel, messageParams);
                return new CommandResponseHandler(message);
            }

            throw new ArgumentException();
        }

        #endregion
    }
}
