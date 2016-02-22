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
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using System;
    using System.Net;
    using System.Web;
    using System.Xml.XPath;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    using HttpRequest = Helpmebot.HttpRequest;

    /// <summary>
    ///     The ACC count.
    /// </summary>
    internal class Acccount : GenericCommand
    {
        #region Constructors and Destructors

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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Acccount(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
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
                username = this.Source.Nickname;
            }

            username = HttpUtility.UrlEncode(username);

            var uri = "http://accounts.wmflabs.org/api.php?action=count&user=" + username;

            string httpResponseData;
            try
            {
                httpResponseData = HttpRequest.Get(uri);
            }
            catch (WebException e)
            {
                this.Log.Warn("Error getting remote data", e);
                return new CommandResponseHandler(e.Message);
            }

            using (var data = httpResponseData.ToStream())
            {
                var xpd = new XPathDocument(data);

                XPathNodeIterator xpni = xpd.CreateNavigator().Select("//user");

                if (xpni.MoveNext())
                {
                    IMessageService messageService = this.CommandServiceHelper.MessageService;
                    if (xpni.Current.GetAttribute("missing", string.Empty) == "true")
                    {
                        string[] msgparams = { username };
                        string msg = messageService.RetrieveMessage("noSuchUser", this.Channel, msgparams);
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

                    string adminmessage = messageService.RetrieveMessage("CmdAccCountAdmin", this.Channel, adminparams);

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

                    string message = messageService.RetrieveMessage("CmdAccCount", this.Channel, messageParams);
                    return new CommandResponseHandler(message);
                }
            }

            throw new ArgumentException();
        }

        #endregion
    }
}