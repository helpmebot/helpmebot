// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommandServiceHelper.cs" company="Helpmebot Development Team">
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

namespace Helpmebot.Commands.Interfaces
{
    using Helpmebot.Configuration;
    using Stwalkerster.IrcClient.Interfaces;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The CommandServiceHelper interface.
    /// </summary>
    public interface ICommandServiceHelper
    {
        #region Public Properties

        /// <summary>
        ///     Gets the client.
        /// </summary>
        IIrcClient Client { get; }

        /// <summary>
        ///     Gets the configuration helper.
        /// </summary>
        IConfigurationHelper ConfigurationHelper { get; }

        /// <summary>
        ///     Gets the media wiki site repository.
        /// </summary>
        IMediaWikiSiteRepository MediaWikiSiteRepository { get; }

        /// <summary>
        ///     Gets the message service.
        /// </summary>
        IMessageService MessageService { get; }

        /// <summary>
        ///     Gets the url shortening service.
        /// </summary>
        IUrlShorteningService UrlShorteningService { get; }

        /// <summary>
        /// Gets the inter-wiki prefix repository.
        /// </summary>
        IInterwikiPrefixRepository InterwikiPrefixRepository { get; }

        /// <summary>
        /// Gets the channel repository.
        /// </summary>
        IChannelRepository ChannelRepository { get; }

        #endregion
    }
}