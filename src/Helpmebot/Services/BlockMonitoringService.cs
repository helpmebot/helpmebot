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
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using helpmebot6.Commands;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;
    using NHibernate.Criterion;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

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

        private readonly IBlockMonitorRepository blockMonitorRepository;

        private readonly Dictionary<string, HashSet<string>> monitors = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockMonitoringService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="mediaWikiSiteRepository">
        /// The media wiki site repository.
        /// </param>
        /// <param name="blockMonitorRepository"></param>
        public BlockMonitoringService(ILogger logger, IMediaWikiSiteRepository mediaWikiSiteRepository,
            IBlockMonitorRepository blockMonitorRepository)
        {
            this.logger = logger;
            this.mediaWikiSiteRepository = mediaWikiSiteRepository;
            this.blockMonitorRepository = blockMonitorRepository;

            // initialise the store
            foreach (var blockMonitor in this.blockMonitorRepository.Get())
            {
                if (!this.monitors.ContainsKey(blockMonitor.MonitorChannel))
                {
                    this.monitors[blockMonitor.MonitorChannel] = new HashSet<string>();
                }

                this.monitors[blockMonitor.MonitorChannel].Add(blockMonitor.ReportChannel);
            }
        }

        public void OnJoinEvent(object sender, JoinEventArgs e)
        {
            this.DoEventProcessing(e.Channel, e.User, (IIrcClient) sender);
        }
        
        /// <inheritdoc />
        [Obsolete] 
        public void DoEventProcessing(string channel, IUser user, IIrcClient client)
        {
            try
            {
                // channel checks
                var alertChannel = this.GetAlertChannel(channel).ToList();
                if (!alertChannel.Any())
                {
                    this.logger.Debug("No block monitoring alert channels found");
                    return;
                }

                var baseWiki = LegacyConfig.Singleton()["baseWiki", channel];
                var mediaWikiSite = this.mediaWikiSiteRepository.GetById(int.Parse(baseWiki));

                
                var ip = this.GetIpAddress(user);
                if (ip == null)
                {
                    this.logger.DebugFormat("Could not detect IP address for user {0}", user);
                }
                else
                {
                    var ipInfo = string.Format(" ({0})", ip);
                    
                    var lookupUrl = string.Format("http://ip-api.com/line/{0}?fields=org,as,status", ip);
                    var textResult = HttpRequest.Get(lookupUrl);
                    var resultData = textResult.Split('\r', '\n');
                    if (resultData.FirstOrDefault() == "success")
                    {
                        ipInfo = string.Format(" ({1}, org: {0})", resultData[1], ip);
                    }
                    
                    var blockInformationData = mediaWikiSite.GetBlockInformation(ip.ToString());
                    
                    foreach (var blockInformation in blockInformationData)
                    {
                        var message = string.Format(
                            "Joined user {0}{4} in channel {1} is IP-blocked ({2}) because: {3}",
                            user.Nickname,
                            channel,
                            blockInformation.Target,
                            blockInformation.BlockReason,
                            ipInfo);
                    
                        foreach (var c in alertChannel)
                        {
                            client.SendMessage(c, message);
                        }
                    }
                }
                
                var userBlockInfo = mediaWikiSite.GetBlockInformation(user.Nickname);
                foreach (var blockInformation in userBlockInfo)
                {
                    var message = string.Format(
                        "Joined user {0} in channel {1} is blocked ({2}) because: {3}",
                        user.Nickname,
                        channel,
                        blockInformation.Target,
                        blockInformation.BlockReason);
                    
                    foreach (var c in alertChannel)
                    {
                        client.SendMessage(c, message);
                    }
                }
                
                

                
                
            }
            catch (Exception ex)
            {
                this.logger.Error("Unknown error occurred in BlockMonitoringService", ex);
            }
        }

        public void AddMap(string monitorChannel, string reportChannel)
        {
            lock (this.monitors)
            {
                var monitor = new BlockMonitor {MonitorChannel = monitorChannel, ReportChannel = reportChannel};
                this.blockMonitorRepository.Save(monitor);

                if (!this.monitors.ContainsKey(monitorChannel))
                {
                    this.monitors[monitorChannel] = new HashSet<string>();
                }

                this.monitors[monitorChannel].Add(reportChannel);
            }
        }

        public void DeleteMap(string monitorChannel, string reportChannel)
        {
            lock (this.monitors)
            {
                this.blockMonitorRepository.Delete(Restrictions.And(
                    Restrictions.Eq("MonitorChannel", monitorChannel),
                    Restrictions.Eq("ReportChannel", reportChannel)
                ));

                this.monitors[monitorChannel].Remove(reportChannel);
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
        private IEnumerable<string> GetAlertChannel(string channel)
        {
            lock (this.monitors)
            {
                if (this.monitors.ContainsKey(channel))
                {
                    return this.monitors[channel];
                }

                return null;
            }
        }
    }
}
