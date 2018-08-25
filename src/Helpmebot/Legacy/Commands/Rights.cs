// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rights.cs" company="Helpmebot Development Team">
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
    using Helpmebot;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    ///     Returns the user rights of a wikipedian
    /// </summary>
    [LegacyCommandFlag(LegacyUserRights.Normal)]
    internal class Rights : GenericCommand
    {
        public Rights(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        protected override CommandResponseHandler ExecuteCommand()
        {
            var crh = new CommandResponseHandler();
            var messageService = this.CommandServiceHelper.MessageService;
            var site = this.GetLocalMediawikiSite();

            string userName;
            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                userName = string.Join(" ", this.Arguments).Trim();
            }
            else
            {
                userName = this.Source.Nickname;
            }

            try
            {
                var rights = site.GetRights(userName);

                string message;
                if (rights != string.Empty)
                {
                    string[] messageParameters = {userName, rights};
                    message = messageService.RetrieveMessage("cmdRightsList", this.Channel, messageParameters);
                }
                else
                {
                    string[] messageParameters = {userName};
                    message = messageService.RetrieveMessage("cmdRightsNone", this.Channel, messageParameters);
                }

                crh.Respond(message);
            }
            catch (MediawikiApiException)
            {
                string[] messageParams = {userName};
                var message = messageService.RetrieveMessage("noSuchUser", this.Channel, messageParams);
                crh.Respond(message);
            }

            return crh;
        }
    }
}