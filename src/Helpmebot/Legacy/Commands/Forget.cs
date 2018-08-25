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
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    ///   Forgets a keyword
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Superuser)]
    internal class Forget : GenericCommand
    {
        /// <summary>
        /// The keyword service.
        /// </summary>
        private readonly IKeywordService keywordService;

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
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Forget(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
            // FIXME: ServiceLocator - keywordservice
            this.keywordService = ServiceLocator.Current.GetInstance<IKeywordService>();
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var messageService = this.CommandServiceHelper.MessageService;
            if (this.Arguments.Length >= 1)
            {
                string forgottenMessage;
                try
                {
                    foreach (var argument in this.Arguments)
                    {
                        this.keywordService.Delete(argument);
                    }

                    forgottenMessage = messageService.RetrieveMessage("cmdForgetDone", this.Channel, null);
                }
                catch (Exception ex)
                {
                    this.Log.Error("Error forgetting keyword", ex);
                    forgottenMessage = messageService.RetrieveMessage("cmdForgetError", this.Channel, null);
                }
                
                this.CommandServiceHelper.Client.SendNotice(this.Source.Nickname, forgottenMessage);
            }
            else
            {
                string[] messageParameters = { "forget", "1", this.Arguments.Length.ToString(CultureInfo.InvariantCulture) };
                this.CommandServiceHelper.Client.SendNotice(
                    this.Source.Nickname,
                    messageService.RetrieveMessage(Messages.NotEnoughParameters, this.Channel, messageParameters));
            }

            return null;
        }
    }
}
