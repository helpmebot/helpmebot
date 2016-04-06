// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockMonitoringService.cs" company="Helpmebot Development Team">
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
//   Defines the BlockMonitoringService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;

    using Castle.Core.Logging;

    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;

    using helpmebot6.Commands;

    /// <summary>
    /// The block monitoring service.
    /// </summary>
    public class BlockMonitoringService : IBlockMonitoringService
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The media wiki site repository.
        /// </summary>
        private readonly IMediaWikiSiteRepository mediaWikiSiteRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockMonitoringService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="mediaWikiSiteRepository">
        /// The media wiki site repository.
        /// </param>
        public BlockMonitoringService(ILogger logger, IMediaWikiSiteRepository mediaWikiSiteRepository)
        {
            this.logger = logger;
            this.mediaWikiSiteRepository = mediaWikiSiteRepository;
        }

        /// <summary>
        /// The do event processing.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        public void DoEventProcessing(string channel, IUser user, IIrcClient client)
        {
            try
            {
                // channel checks
                var alertChannel = this.GetAlertChannel(channel);
                if (alertChannel == null)
                {
                    return;
                }

                var ip = this.GetIpAddress(user);

                if (ip == null)
                {
                    return;
                }

                string baseWiki = LegacyConfig.Singleton()["baseWiki", channel];

                MediaWikiSite mediaWikiSite = this.mediaWikiSiteRepository.GetById(int.Parse(baseWiki));

                BlockInformation blockInformation = mediaWikiSite.GetBlockInformation(ip.ToString()).FirstOrDefault();

                if (blockInformation.Id != null)
                {
                    string orgname = null;

                    var textResult = HttpRequest.Get(string.Format("http://ip-api.com/line/{0}?fields=org,as,status", ip));
                    var resultData = textResult.Split('\r', '\n');
                    if (resultData.FirstOrDefault() == "success")
                    {
                        orgname = string.Format(", org: {0}", resultData[1]);
                    }

                    var message = string.Format(
                        "Joined user {0} ({4}{5}) in channel {1} is blocked ({2}) because: {3}",
                        user.Nickname,
                        channel,
                        blockInformation.Target,
                        blockInformation.BlockReason,
                        ip,
                        orgname);

                    client.SendMessage(alertChannel, message);
                }
                else
                {
                    var textResult = HttpRequest.Get(string.Format("http://ip-api.com/line/{0}?fields=org,as,status", ip));
                    var resultData = textResult.Split('\r', '\n');
                    if (resultData.FirstOrDefault() == "success")
                    {
                        var message = string.Format(
                            "Joined user {0} ({2}{3}) in channel {1}",
                            user.Nickname,
                            channel,
                            ip,
                            resultData[1]);

                        client.SendMessage(alertChannel, message);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error("Unknown error occurred in BlockMonitoringService", ex);
            }
        }

        /// <summary>
        /// The get ip address.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="IPAddress"/>.
        /// </returns>
        private IPAddress GetIpAddress(IUser user)
        {
            if (user.Hostname.Contains("/"))
            {
                // cloaked. hmm...
                // gateway, try username
                var validHexIp = new Regex("^[0-9A-Fa-f]{8}$");

                if (validHexIp.IsMatch(user.Username))
                {
                    return Decode.GetIpAddressFromHex(user.Username);
                }

                // not a gateway cloak. Can't do anything.
                return null;
            }

            // resolve hostname
            IPAddress[] addresses = new IPAddress[0];
            try
            {
                addresses = Dns.GetHostEntry(user.Hostname).AddressList;
            }
            catch (SocketException)
            {
            }

            return addresses.FirstOrDefault();
        }

        /// <summary>
        /// The get alert channel.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetAlertChannel(string channel)
        {
            if (channel == "##stwalkerster")
            {
                return "##stwalkerster";
            }

            if (channel == "##stwalkerster-development")
            {
                return "##stwalkerster";
            }

            if (channel == "#wikipedia-en-help")
            {
                return "##helpmebot";
            }

            return null;
        }
    }
}
