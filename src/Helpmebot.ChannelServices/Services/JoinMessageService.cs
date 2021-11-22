// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JoinMessageService.cs" company="Helpmebot Development Team">
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
//   Defines the JoinMessageService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Configuration;
    using Helpmebot.ChannelServices.Model;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using Prometheus;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Messages;
    using Stwalkerster.IrcClient.Model.Interfaces;
    using Cache = System.Collections.Generic.Dictionary<string, Model.RateLimitCacheEntry>;
    using ModuleConfiguration = Helpmebot.ChannelServices.Configuration.ModuleConfiguration;

    public class JoinMessageService : IJoinMessageService
    {
        private static readonly Counter WelcomerActivations = Metrics.CreateCounter(
            "helpmebot_welcomer_triggers_total",
            "Number of welcomer activations",
            new CounterConfiguration
            {
                LabelNames = new[] {"channel"}
            });

        private readonly Dictionary<string, Cache> rateLimitCache = new Dictionary<string, Cache>();
        private readonly ILogger logger;
        private readonly IResponder responder;
        private readonly ISession session;
        private readonly RateLimitConfiguration configuration;
        private readonly IGeolocationService geolocationService;
        private readonly IIrcClient client;
        private readonly IBlockMonitoringService blockMonitoringService;

        private readonly Dictionary<string, List<ExemptListEntry>> appliedExemptions =
            new Dictionary<string, List<ExemptListEntry>>();

        public JoinMessageService(
            ILogger logger,
            IResponder responder,
            ISession session,
            ModuleConfiguration configuration,
            IGeolocationService geolocationService,
            IIrcClient client,
            IBlockMonitoringService blockMonitoringService)
        {
            this.logger = logger;
            this.responder = responder;
            this.session = session;
            this.configuration = configuration.JoinMessageRateLimits;
            this.geolocationService = geolocationService;
            this.client = client;
            this.blockMonitoringService = blockMonitoringService;

            this.blockMonitoringService.JoinMessageService = this;
        }

        public void OnJoinEvent(object sender, JoinEventArgs e)
        {
            if (e.User.Nickname == e.Client.Nickname)
            {
                this.logger.InfoFormat("Seen self join on channel {0}, not welcoming.", e.Channel);
                return;
            }

            try
            {
                this.DoWelcome(e.User, e.Channel);
            }
            catch (Exception exception)
            {
                this.logger.Error("Exception encountered in WelcomeNewbieOnJoinEvent", exception);
            }
        }

        private void DoWelcome(IUser networkUser, string channel)
        {
            // Rate limit this per hostname/channel. Make sure this matches the exemption setting below.
            if (this.RateLimit(networkUser.Hostname, channel))
            {
                return;
            }

            // status
            bool match = false;

            this.logger.DebugFormat("Searching for welcome matches for {0} in {1}...", networkUser, channel);

            var users = this.GetWelcomeUsers(channel);

            if (users.Any())
            {
                foreach (var welcomeUser in users)
                {
                    Match nick = new Regex(welcomeUser.Nick).Match(networkUser.Nickname);
                    Match user = new Regex(welcomeUser.User).Match(networkUser.Username);
                    Match host = new Regex(welcomeUser.Host).Match(networkUser.Hostname);
                    Match account = new Regex(welcomeUser.Account).Match(networkUser.Account ?? string.Empty);

                    if (nick.Success && user.Success && host.Success && account.Success)
                    {
                        this.logger.DebugFormat(
                            "Found a match for {0} in {1} with {2}",
                            networkUser,
                            channel,
                            welcomeUser);
                        match = true;
                        break;
                    }
                }
            }

            if (!match)
            {
                this.logger.InfoFormat("No welcome matches found for {0} in {1}.", networkUser, channel);
                return;
            }

            this.logger.DebugFormat("Searching for exception matches for {0} in {1}...", networkUser, channel);

            var exceptions = this.GetExceptions(channel);

            if (exceptions.Any())
            {
                foreach (var welcomeUser in exceptions)
                {
                    Match nick = new Regex(welcomeUser.Nick).Match(networkUser.Nickname);
                    Match user = new Regex(welcomeUser.User).Match(networkUser.Username);
                    Match host = new Regex(welcomeUser.Host).Match(networkUser.Hostname);
                    Match account = new Regex(welcomeUser.Account).Match(networkUser.Account ?? string.Empty);

                    if (nick.Success && user.Success && host.Success && account.Success)
                    {
                        this.logger.DebugFormat(
                            "Found an exception match for {0} in {1} with {2}",
                            networkUser,
                            channel,
                            welcomeUser);

                        return;
                    }
                }
            }

            this.SendWelcome(networkUser, channel, this.client);
        }

        public void SendWelcome(IUser networkUser, string channel, IIrcClient client)
        {
            var welcomeOverride = this.GetOverride(channel);
            var applyOverride  = false;

            if (welcomeOverride != null)
            {
                applyOverride = this.DoesOverrideApply(networkUser, welcomeOverride);
            }
            
            
            if (applyOverride)
            {
                this.logger.WarnFormat("Detected applicable override, firing alternate welcome");

                if (welcomeOverride.Message != null)
                {
                    var welcomeMessage = this.responder.Respond(
                        welcomeOverride.Message,
                        channel,
                        new[] {networkUser.Nickname, channel});

                    foreach (var message in welcomeMessage)
                    {
                        client.SendMessage(channel, message.CompileMessage());
                    }
                }
            } 
            else
            {
                // Either no override defined, or override not matching.
                this.logger.InfoFormat("Welcoming {0} into {1}...", networkUser, channel);
                
                if (welcomeOverride != null && welcomeOverride.ExemptNonMatching && client.Channels[channel].Users[client.Nickname].Operator)
                {
                    var modeTarget = $"*!*@{networkUser.Hostname}";

                    lock (this.appliedExemptions)
                    {
                        if (!this.appliedExemptions.ContainsKey(channel))
                        {
                            this.appliedExemptions.Add(channel, new List<ExemptListEntry>());
                        }

                        this.appliedExemptions[channel]
                            .Add(new ExemptListEntry { User = networkUser, Exemption = modeTarget });
                    }
                    
                    client.Mode(channel, "+e " + modeTarget);

                    var notificationTarget = this.GetCrossChannelNotificationTarget(channel);
                    if (notificationTarget != null)
                    {
                        client.SendMessage(
                            notificationTarget,
                            $"Auto-exempting {networkUser.Nickname}. Please alert ops if there are issues. (Ops:  /mode {channel} -e {modeTarget}  )");
                    }
                }

                var welcomeMessage = this.responder.Respond(
                    "channelservices.welcomer.welcome",
                    channel,
                    new object[] { networkUser.Nickname, channel });

                foreach (var message in welcomeMessage)
                {
                    client.SendMessage(channel, message.CompileMessage());                    
                }
            }

            WelcomerActivations.WithLabels(channel).Inc();
        }

        private bool DoesOverrideApply(IUser networkUser, WelcomerOverride welcomeOverride)
        {
            bool matches = true;
            
            IPAddress clientip = null;
                
            var userMatch = Regex.Match(networkUser.Username, "^[a-fA-F0-9]{8}$");
            if (userMatch.Success)
            {
                // We've got a hex-encoded IP.
                clientip = networkUser.Username.GetIpAddressFromHex();
            }

            if (!networkUser.Hostname.Contains("/"))
            {
                // real hostname, not a cloak
                var hostAddresses = Dns.GetHostAddresses(networkUser.Hostname);
                if (hostAddresses.Length > 0)
                {
                    clientip = hostAddresses.First();
                }
            }
            
            // checks
            
            if (welcomeOverride.Geolocation != null && clientip != null)
            {
                matches &= this.geolocationService.GetLocation(clientip).Country == welcomeOverride.Geolocation;
            }

            if (welcomeOverride.BlockMessage != null)
            {
                Regex blockMessageRegex = new Regex(welcomeOverride.BlockMessage, RegexOptions.IgnoreCase);
                var blockDatas = this.blockMonitoringService.GetBlockData(networkUser, welcomeOverride.Channel.Name);
                matches &= blockDatas.Any(x => blockMessageRegex.IsMatch(x.BlockInformation.BlockReason));
            }

            return matches;
        }

        public virtual IList<WelcomeUser> GetExceptions(string channel)
        {
            var exceptions = this.session.QueryOver<WelcomeUser>()
                .Where(x => x.Channel == channel && x.Exception)
                .List();
            return exceptions;
        }

        public virtual string GetCrossChannelNotificationTarget(string channel)
        {
            Channel frontendAlias = null, backendAlias = null;

            var crossChannel = this.session.QueryOver<CrossChannel>()
                .Inner.JoinAlias(x => x.FrontendChannel, () => frontendAlias)
                .Inner.JoinAlias(x => x.BackendChannel, () => backendAlias)
                .Where(x => frontendAlias.Name == channel)
                .SingleOrDefault();
            
            return crossChannel?.BackendChannel?.Name;
        }
        
        public virtual IList<WelcomeUser> GetWelcomeUsers(string channel)
        {
            var users = this.session.QueryOver<WelcomeUser>()
                .Where(x => x.Channel == channel && x.Exception == false)
                .List();
            return users;
        }

        public virtual WelcomerOverride GetOverride(string channel)
        {
            Channel channelAlias = null;
            var overrides = this.session.QueryOver<WelcomerOverride>()
                .Inner.JoinAlias(x => x.Channel, () => channelAlias)
                .Where(x => x.ActiveFlag == channelAlias.WelcomerFlag)
                .And(() => channelAlias.Name == channel)
                .List();

            return overrides.FirstOrDefault();
        }
        
        public void ClearRateLimitCache()
        {
            lock (this.rateLimitCache)
            {
                this.rateLimitCache.Clear();
            }
        }
        
        private bool RateLimit(string hostname, string channel)
        {
            if (string.IsNullOrEmpty(hostname) || string.IsNullOrEmpty(channel))
            {
                // sanity check - this probably chanserv.
                this.logger.Error("JoinMessage ratelimiting called with null channel or null hostname!");
                return true;
            }

            try
            {
                // TODO: rate limiting needs to be tidyed up a bit
                lock (this.rateLimitCache)
                {
                    if (!this.rateLimitCache.ContainsKey(channel))
                    {
                        this.rateLimitCache.Add(channel, new Cache());
                    }

                    var channelCache = this.rateLimitCache[channel];

                    if (channelCache.ContainsKey(hostname))
                    {
                        this.logger.Debug("Rate limit key found.");

                        var cacheEntry = channelCache[hostname];

                        if (cacheEntry.Expiry.AddMinutes(this.configuration.RateLimitDuration) >= DateTime.Now)
                        {
                            this.logger.Debug("Rate limit key NOT expired.");

                            if (cacheEntry.Counter >= this.configuration.RateLimitMax)
                            {
                                this.logger.Debug("Rate limit HIT");

                                // RATE LIMITED!
                                return true;
                            }

                            this.logger.Debug("Rate limit incremented.");

                            // increment counter
                            cacheEntry.Counter++;
                        }
                        else
                        {
                            this.logger.Debug("Rate limit key is expired, resetting to new value.");

                            // Cache expired
                            cacheEntry.Expiry = DateTime.Now;
                            cacheEntry.Counter = 1;
                        }
                    }
                    else
                    {
                        this.logger.Debug("Rate limit not found, creating key.");

                        // Not in cache.
                        var cacheEntry = new RateLimitCacheEntry {Expiry = DateTime.Now, Counter = 1};
                        channelCache.Add(hostname, cacheEntry);
                    }

                    // Clean up the channel's cache.
                    foreach (var key in channelCache.Keys.ToList())
                    {
                        if (channelCache[key].Expiry.AddMinutes(this.configuration.RateLimitDuration) < DateTime.Now)
                        {
                            // Expired.
                            channelCache.Remove(key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ooh dear. Something went wrong with rate limiting.
                this.logger.Error("Unknown error during rate limit processing.", ex);
                return false;
            }

            return false;
        }

        public void RemoveExemption(string channel, IUser user, bool priority = false)
        {
            List<ExemptListEntry> exemptions;
            
            lock (this.appliedExemptions)
            {
                if (!this.appliedExemptions.ContainsKey(channel))
                {
                    return;
                }
                
                exemptions = this.appliedExemptions[channel].Where(x => x.User.Equals(user)).ToList();
                exemptions.ForEach(x => this.appliedExemptions[channel].Remove(x));
            }

            if (!this.client.Channels[channel].Users[this.client.Nickname].Operator)
            {
                return;
            }

            foreach (var exemption in exemptions)
            {
                var message = new Message("MODE", new[] { channel, "-e", exemption.Exemption });
                if (priority)
                {
                    this.client.PrioritySend(message);
                }
                else
                {
                    this.client.Send(message);
                }
            }
        }
        
        public void Start()
        {
            this.logger.Debug("Starting join message service");
            this.client.JoinReceivedEvent += this.OnJoinEvent;
            this.client.QuitReceivedEvent += this.OnQuitEvent;
            this.client.KickReceivedEvent += this.OnKickEvent;
            this.client.PartReceivedEvent += this.OnPartEvent;
        }

        public void Stop()
        {
            this.logger.Debug("Stopping join message service");
            this.client.JoinReceivedEvent -= this.OnJoinEvent;
            this.client.QuitReceivedEvent -= this.OnQuitEvent;
            this.client.KickReceivedEvent -= this.OnKickEvent;
            this.client.PartReceivedEvent -= this.OnPartEvent;
        }

        private void OnPartEvent(object sender, JoinEventArgs e)
        {
            this.RemoveExemption(e.Channel, e.User);
        }

        private void OnKickEvent(object sender, KickEventArgs e)
        {
            this.RemoveExemption(e.Channel, e.User);
        }

        private void OnQuitEvent(object sender, QuitEventArgs e)
        {
            List<string> appliedExemptionsKeys;
            
            lock (this.appliedExemptions)
            {
                appliedExemptionsKeys = this.appliedExemptions.Keys.ToList();
            }
            
            foreach (var channel in appliedExemptionsKeys)
            {
                this.RemoveExemption(channel, e.User);    
            }
        }
    }
}