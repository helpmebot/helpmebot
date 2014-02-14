// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Forget.cs" company="Helpmebot Development Team">
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
//   Forgets a keyword
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;

    using Helpmebot;
    using Helpmebot.Legacy.IRC;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///   Forgets a keyword
    /// </summary>
    internal class Forget : GenericCommand
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
        /// Initialises a new instance of the <see cref="Forget"/> class.
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
        public Forget(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
            // FIXME: ServiceLocator
            this.keywordService = ServiceLocator.Current.GetInstance<IKeywordService>();
            this.ircAccessLayer = ServiceLocator.Current.GetInstance<IIrcAccessLayer>();
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length >= 1)
            {
                string forgottenMessage;
                try
                {
                    foreach (var argument in this.Arguments)
                    {
                        this.keywordService.Delete(argument);
                    }

                    forgottenMessage = this.MessageService.RetrieveMessage("cmdForgetDone", this.Channel, null);
                }
                catch (Exception ex)
                {
                    this.Log.Error("Error forgetting keyword", ex);
                    forgottenMessage = this.MessageService.RetrieveMessage("cmdForgetError", this.Channel, null);
                }
                
                this.ircAccessLayer.IrcNotice(this.Source.Nickname, forgottenMessage);
            }
            else
            {
                string[] messageParameters = { "forget", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture) };
                this.ircAccessLayer.IrcNotice(
                    this.Source.Nickname,
                    this.MessageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            return null;
        }
    }
}
