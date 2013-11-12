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
// <summary>
//   Defines the Accstatus type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Xml.XPath;

    using Helpmebot;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The status of ACC.
    /// </summary>
    internal class Accstatus : GenericCommand
    {
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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Accstatus(User source, string channel, string[] args, IMessageService messageService)
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
            XPathDocument xpd =
                new XPathDocument(
                    HttpRequest.get("http://toolserver.org/~acc/api.php?action=status"));

            XPathNodeIterator xpni = xpd.CreateNavigator().Select("//status");

            if (xpni.MoveNext())
            {
                string[] messageParams =
                    {
                        xpni.Current.GetAttribute("open", string.Empty),
                        xpni.Current.GetAttribute("admin", string.Empty),
                        xpni.Current.GetAttribute("checkuser", string.Empty),
                        xpni.Current.GetAttribute("bans", string.Empty),
                        xpni.Current.GetAttribute("useradmin", string.Empty),
                        xpni.Current.GetAttribute("user", string.Empty),
                        xpni.Current.GetAttribute("usernew", string.Empty)
                    };

                string message = new Message().GetMessage("CmdAccStatus", messageParams);
                return new CommandResponseHandler(message);
            }

            throw new ArgumentException();
        }

        #endregion
    }
}
