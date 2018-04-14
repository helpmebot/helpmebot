// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandServiceHelper.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Commands
{
    using Helpmebot.Commands.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///     The command service helper.
    /// </summary>
    public class CommandServiceHelper : ICommandServiceHelper
    {
        #region Fields

        /// <summary>
        /// The channel repository.
        /// </summary>
        private readonly IChannelRepository channelRepository;

        /// <summary>
        ///     The client.
        /// </summary>
        private readonly IIrcClient client;

        /// <summary>
        ///     The inter-wiki prefix repository.
        /// </summary>
        private readonly IInterwikiPrefixRepository interwikiPrefixRepository;

        /// <summary>
        ///     The media wiki site repository.
        /// </summary>
        private readonly IMediaWikiSiteRepository mediaWikiSiteRepository;

        /// <summary>
        ///     The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        ///     The url shortening service.
        /// </summary>
        private readonly IUrlShorteningService urlShorteningService;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the CommandServiceHelper class.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="messageService">
        /// The message service.
        /// </param>
        /// <param name="urlShorteningService">
        /// The url Shortening Service.
        /// </param>
        /// <param name="mediaWikiSiteRepository">
        /// The media Wiki Site Repository.
        /// </param>
        /// <param name="interwikiPrefixRepository">
        /// The inter-wiki Prefix Repository.
        /// </param>
        /// <param name="channelRepository">
        /// The channel Repository.
        /// </param>
        public CommandServiceHelper(
            IIrcClient client, 
            IMessageService messageService, 
            IUrlShorteningService urlShorteningService, 
            IMediaWikiSiteRepository mediaWikiSiteRepository, 
            IInterwikiPrefixRepository interwikiPrefixRepository, 
            IChannelRepository channelRepository)
        {
            this.client = client;
            this.messageService = messageService;
            this.urlShorteningService = urlShorteningService;
            this.mediaWikiSiteRepository = mediaWikiSiteRepository;
            this.interwikiPrefixRepository = interwikiPrefixRepository;
            this.channelRepository = channelRepository;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the channel repository.
        /// </summary>
        public IChannelRepository ChannelRepository
        {
            get
            {
                return this.channelRepository;
            }
        }

        /// <summary>
        ///     Gets the client.
        /// </summary>
        public IIrcClient Client
        {
            get
            {
                return this.client;
            }
        }

        /// <summary>
        ///     Gets the inter-wiki prefix repository.
        /// </summary>
        public IInterwikiPrefixRepository InterwikiPrefixRepository
        {
            get
            {
                return this.interwikiPrefixRepository;
            }
        }

        /// <summary>
        ///     Gets the media wiki site repository.
        /// </summary>
        public IMediaWikiSiteRepository MediaWikiSiteRepository
        {
            get
            {
                return this.mediaWikiSiteRepository;
            }
        }

        /// <summary>
        ///     Gets the message service.
        /// </summary>
        public IMessageService MessageService
        {
            get
            {
                return this.messageService;
            }
        }

        /// <summary>
        ///     Gets the url shortening service.
        /// </summary>
        public IUrlShorteningService UrlShorteningService
        {
            get
            {
                return this.urlShorteningService;
            }
        }

        #endregion
    }
}