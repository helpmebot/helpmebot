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

namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Prometheus;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// The block monitoring service.
    /// </summary>
    public class BlockMonitoringService : IBlockMonitoringService
    {
        private static readonly Counter BlockMonitorEventsCounter = Metrics.CreateCounter(
            "helpmebot_blockmonitor_events_total",
            "Number of block monitor events",
            new CounterConfiguration
            {
                LabelNames = new[] {"channel"}
            });
        
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        private readonly ILinkerService linkerService;
        private readonly IUrlShorteningService urlShorteningService;
        private readonly ISession globalSession;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly BotConfiguration botConfiguration;
        private readonly IWebServiceClient webServiceClient;
        private readonly IIrcClient client;
        private readonly ITrollMonitoringService trollMonitoringService;

        private readonly Dictionary<string, HashSet<string>> monitors = new Dictionary<string, HashSet<string>>();

        public BlockMonitoringService(
            ILogger logger,
            ILinkerService linkerService,
            IUrlShorteningService urlShorteningService,
            ISession globalSession,
            IMediaWikiApiHelper apiHelper,
            BotConfiguration botConfiguration,
            IWebServiceClient webServiceClient,
            IIrcClient client,
            ITrollMonitoringService trollMonitoringService)
        {
            this.logger = logger;
            this.linkerService = linkerService;
            this.urlShorteningService = urlShorteningService;
            this.globalSession = globalSession;
            this.apiHelper = apiHelper;
            this.botConfiguration = botConfiguration;
            this.webServiceClient = webServiceClient;
            this.client = client;
            this.trollMonitoringService = trollMonitoringService;

            // initialise the store
            foreach (var blockMonitor in globalSession.CreateCriteria<BlockMonitor>().List<BlockMonitor>())
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
            try
            {
                // channel checks
                var alertChannelEnumerable = this.GetAlertChannel(e.Channel);
                if (alertChannelEnumerable == null)
                {
                    this.logger.Debug("No block monitoring alert channels found");
                    return;
                }

                var alertChannel = alertChannelEnumerable.ToList();

                if (!alertChannel.Any())
                {
                    this.logger.Debug("No block monitoring alert channels found");
                    return;
                }

                MediaWikiSite mediaWikiSite;
                using (var tx = this.globalSession.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    mediaWikiSite = this.globalSession.CreateCriteria<Channel>()
                        .Add(Restrictions.Eq("Name", e.Channel))
                        .UniqueResult<Channel>().BaseWiki;
                    
                    tx.Rollback();
                }

                var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSite);
                
                bool triggered = false;
                
                var ip = this.GetIpAddress(e.User);
                if (ip == null)
                {
                    this.logger.DebugFormat("Could not detect IP address for user {0}", e.User);
                }
                else
                {
                    var ipInfo = string.Format(" ({0})", ip);

                    var queryParameters = new NameValueCollection
                    {
                        {"fields", "org,as,status"}
                    };

                    var lookupUrl = string.Format("http://ip-api.com/line/{0}", ip);

                    var httpResponseData = this.webServiceClient.DoApiCall(
                        queryParameters,
                        lookupUrl,
                        this.botConfiguration.UserAgent);

                    var textResult = new StreamReader(httpResponseData).ReadToEnd();
                    var resultData = textResult.Split('\r', '\n');
                    if (resultData.FirstOrDefault() == "success")
                    {
                        ipInfo = string.Format(" ({1}, org: {0})", resultData[1], ip);
                    }

                    var blockInformationData = mediaWikiApi.GetBlockInformation(ip.ToString());

                    foreach (var blockInformation in blockInformationData)
                    {
                        triggered = true;
                        foreach (var c in alertChannel)
                        {
                            var url = this.linkerService.ConvertWikilinkToUrl(c, "Special:Contributions/" + blockInformation.Target);
                            url = this.urlShorteningService.Shorten(url);

                            var message = string.Format(
                                "Joined user {0}{4} in channel {1} is IP-blocked ({2}) because: {3} ( {5} )",
                                e.User.Nickname,
                                e.Channel,
                                blockInformation.Target,
                                blockInformation.BlockReason,
                                ipInfo,
                                url);
                            ((IIrcClient) sender).SendMessage(c, message);
                        }
                    }
                }

                var userBlockInfo = mediaWikiApi.GetBlockInformation(e.User.Nickname);
                foreach (var blockInformation in userBlockInfo)
                {
                    triggered = true;
                    foreach (var c in alertChannel)
                    {
                        var url = this.linkerService.ConvertWikilinkToUrl(c, "Special:Contributions/" + blockInformation.Target);
                        url = this.urlShorteningService.Shorten(url);

                        var message = string.Format(
                            "Joined user {0} in channel {1} is blocked ({2}) because: {3} ( {4} )",
                            e.User.Nickname,
                            e.Channel,
                            blockInformation.Target,
                            blockInformation.BlockReason,
                            url);

                        ((IIrcClient) sender).SendMessage(c, message);

                        if (e.Channel == "#wikipedia-en-help")
                        {
                            this.trollMonitoringService.ForceAddTracking(e.User, this);
                        }
                    }
                }

                if (triggered)
                {
                    BlockMonitorEventsCounter.WithLabels(e.Channel).Inc();
                }
            }
            catch (WebException ex)
            {
                this.logger.Warn("Error fetching from API", ex);
            }
            catch (Exception ex)
            {
                this.logger.Error("Unknown error occurred in BlockMonitoringService", ex);
            }
        }

        public void AddMap(string monitorChannel, string reportChannel, ISession databaseSession)
        {
            lock (this.monitors)
            {
                var monitor = new BlockMonitor {MonitorChannel = monitorChannel, ReportChannel = reportChannel};

                using(var txn = databaseSession.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    databaseSession.SaveOrUpdate(monitor);
                    txn.Commit();
                }

                if (!this.monitors.ContainsKey(monitorChannel))
                {
                    this.monitors[monitorChannel] = new HashSet<string>();
                }

                this.monitors[monitorChannel].Add(reportChannel);
            }
        }

        public void DeleteMap(string monitorChannel, string reportChannel, ISession databaseSession)
        {
            lock (this.monitors)
            {
                using (var txn = databaseSession.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var deleteList = databaseSession.CreateCriteria<BlockMonitor>()
                        .Add(
                            Restrictions.And(
                                Restrictions.Eq("MonitorChannel", monitorChannel),
                                Restrictions.Eq("ReportChannel", reportChannel)
                            ))
                        .List<BlockMonitor>();

                    foreach (var monitor in deleteList)
                    {
                        this.logger.DebugFormat(
                            "Deleting monitor {2} for {0} to {1}",
                            monitor.MonitorChannel,
                            monitor.ReportChannel,
                            monitor.Id);
                        databaseSession.Delete(monitor);
                    }

                    txn.Commit();
                }

                var success = this.monitors[monitorChannel].Remove(reportChannel);
                if (!success)
                {
                    this.logger.WarnFormat("Could not remove tracking for {0} in {1}", monitorChannel, reportChannel);
                }
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
                    return user.Username.GetIpAddressFromHex();
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
        
        public void Start()
        {
            this.logger.Debug("Starting block monitoring service");
            this.client.JoinReceivedEvent += this.OnJoinEvent;
        }

        public void Stop()
        {
            this.logger.Debug("Stopping block monitoring service");
            this.client.JoinReceivedEvent -= this.OnJoinEvent;
        }
    }
}