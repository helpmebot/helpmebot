// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Learn.cs" company="Helpmebot Development Team">
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
//   Learns a keyword
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;
    using System.Linq;

    using Helpmebot;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.IRC;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///   Learns a keyword
    /// </summary>
    internal class Learn : GenericCommand
    {
        /// <summary>
        /// The keyword service.
        /// </summary>
        private readonly IKeywordService keywordService;

        /// <summary>
        /// The IRC access layer.
        /// </summary>
        private readonly IIrcAccessLayer ircAccessLayer;

        /// <summary>
        /// Initialises a new instance of the <see cref="Learn"/> class.
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
        public Learn(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
            // FIXME: ServiceLocator
            this.keywordService = ServiceLocator.Current.GetInstance<IKeywordService>();
            this.ircAccessLayer = ServiceLocator.Current.GetInstance<IIrcAccessLayer>();
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var action = false;
            var args = this.Arguments.ToList();

            if (args[0] == "@action")
            {
                action = true;
                args.PopFromFront();
            }

            if (args.Count >= 2)
            {
                var keywordName = args.PopFromFront();
                string message;

                try
                {
                    this.keywordService.Create(keywordName, args.Implode(), action);
                    message = this.MessageService.RetrieveMessage("cmdLearnDone", this.Channel, null);
                }
                catch (Exception ex)
                {
                    message = this.MessageService.RetrieveMessage("cmdLearnError", this.Channel, null);
                    this.Log.Error("Error learning command", ex);
                }

                this.ircAccessLayer.IrcNotice(this.Source.Nickname, message);
            }
            else
            {
                string[] messageParameters = { "learn", "2", args.Count.ToString(CultureInfo.InvariantCulture) };
                this.ircAccessLayer.IrcNotice(
                    this.Source.Nickname,
                    this.MessageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            return null;
        }
    }
}
