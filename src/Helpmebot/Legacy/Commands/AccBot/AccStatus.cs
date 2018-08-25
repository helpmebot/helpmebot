// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccStatus.cs" company="Helpmebot Development Team">
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
    using System.Xml.XPath;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;

    /// <summary>
    ///     The status of ACC.
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Advanced)]
    internal class Accstatus : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Accstatus"/> class.
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
        public Accstatus(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
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
            string httpResponseData;

            try
            {
                httpResponseData = HttpRequest.Get("https://accounts.wmflabs.org/api.php?action=status");
            }
            catch (WebException e)
            {
                this.Log.Warn("Error getting remote data", e);
                return new CommandResponseHandler(e.Message);
            }

            using (var data = httpResponseData.ToStream())
            {
                var xpd = new XPathDocument(data);

                XPathNodeIterator xpni = xpd.CreateNavigator().Select("//status");

                if (xpni.MoveNext())
                {
                    string[] messageParams =
                        {
                            xpni.Current.GetAttribute("open", string.Empty),
                            xpni.Current.GetAttribute("admin", string.Empty),
                            xpni.Current.GetAttribute("checkuser", string.Empty),
                            xpni.Current.GetAttribute("hold", string.Empty),
                            xpni.Current.GetAttribute("proxy", string.Empty),

                            xpni.Current.GetAttribute("bans", string.Empty),

                            xpni.Current.GetAttribute("useradmin", string.Empty),
                            xpni.Current.GetAttribute("user", string.Empty),
                            xpni.Current.GetAttribute("usernew", string.Empty)
                        };

                    string message = this.CommandServiceHelper.MessageService.RetrieveMessage(
                        "CmdAccStatus",
                        this.Channel,
                        messageParams);
                    return new CommandResponseHandler(message);
                }
            }

            throw new ArgumentException();
        }

        #endregion
    }
}