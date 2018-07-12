// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Editcount.cs" company="Helpmebot Development Team">
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
    using System.Globalization;
    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;

    /// <summary>
    ///     Returns the edit count of a Wikipedian
    /// </summary>
    internal class Editcount : GenericCommand
    {
        public Editcount(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        protected override CommandResponseHandler ExecuteCommand()
        {
            string userName;
            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                userName = string.Join(" ", this.Arguments).Trim();
            }
            else
            {
                userName = this.Source.Nickname;
            }

            
            var channelRepository = this.CommandServiceHelper.ChannelRepository;
            var channel = channelRepository.GetByName(this.Channel);

            var mediaWikiSite = this.CommandServiceHelper.MediaWikiSiteRepository.GetById(channel.BaseWiki);
            var messageService = this.CommandServiceHelper.MessageService;
            try
            {
                var editCount = mediaWikiSite.GetEditCount(userName);

                var xtoolsUrl = string.Format(
                    "https://tools.wmflabs.org/xtools-ec/index.php?user={0}&project=en.wikipedia.org",
                    userName);
                var xtoolsShortUrl = this.CommandServiceHelper.UrlShorteningService.Shorten(xtoolsUrl);

                string[] messageParameters =
                    {editCount.ToString(CultureInfo.InvariantCulture), userName, xtoolsShortUrl};

                var message = messageService.RetrieveMessage("editCount", this.Channel, messageParameters);

                return new CommandResponseHandler(message);
            }
            catch (MediawikiApiException)
            {
                string[] messageParams = { userName };
                var message = messageService.RetrieveMessage("noSuchUser", this.Channel, messageParams);
                return new CommandResponseHandler(message);
            }
        }
    }
}