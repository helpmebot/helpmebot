namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Castle.Core.Logging;
    using DnsClient;
    using DnsClient.Protocol;
    using Helpmebot.ChannelServices.Configuration;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class RangeLookupService : IRangeLookupService
    {
        private readonly ILogger logger;
        private readonly IIrcClient client;
        private readonly ModuleConfiguration configuration;
        private readonly IResponder responder;
        private readonly LookupClient dnsClient;
        

        private readonly Dictionary<IPNetwork2, string> knownRanges;

        public RangeLookupService(ILogger logger, IIrcClient client, ModuleConfiguration configuration, IResponder responder)
        {
            this.logger = logger;
            this.client = client;
            this.configuration = configuration;
            this.responder = responder;

            var options = new LookupClientOptions {UseCache = false, EnableAuditTrail = true};
            this.dnsClient = new LookupClient(options);

            this.knownRanges =
                this.configuration.AlertOnRanges.ToDictionary(pair => IPNetwork2.Parse(pair.Key), pair => pair.Value);
        }
        
        private void OnJoinEvent(object sender, JoinEventArgs e)
        {
            if (e.User.Nickname == e.Client.Nickname)
            {
                this.logger.InfoFormat("Seen self join on channel {0}, not checking range.", e.Channel);
                return;
            }

            if (e.Channel != this.configuration.TrollManagement.TargetChannel)
            {
                this.logger.DebugFormat("Seen join on channel {0}, which is not our target channel.", e.Channel);
                return;
            }
            
            try
            {
                this.CheckRange(e.User);
            }
            catch (Exception exception)
            {
                this.logger.Error("Exception encountered in RangeLookupService::OnJoinEvent", exception);
            }
        }

        private void CheckRange(IUser user)
        {
            if (user.Hostname.Contains("/"))
            {
                // user is cloaked
                return;
            }

            List<IPAddress> ipAddresses;
            if (IPAddress.TryParse(user.Hostname, out var addr))
            {
                ipAddresses = new List<IPAddress> { addr };
            }
            else
            {
                ipAddresses = this.DoDnsLookup(user.Hostname);    
            }

            foreach (var ip in ipAddresses)
            {
                foreach (var range in this.knownRanges)
                {
                    if (range.Key.Contains(ip))
                    {
                        var message = this.responder.GetMessagePart(
                            "channelservices.rangelookup.hit",
                            this.configuration.TrollManagement.PublicAlertChannel,
                            new object[]
                            {
                                user,
                                range.Key,
                                range.Value
                            });
                        this.client.SendMessage(this.configuration.TrollManagement.PublicAlertChannel, message);
                    }
                }
            }
        }

        private List<IPAddress> DoDnsLookup(string query)
        {
            var aQueryResponse = this.dnsClient.Query(query, QueryType.A);
            var aaaaQueryResponse = this.dnsClient.Query(query, QueryType.AAAA);
            
            var ipAddresses = new List<IPAddress>();

            if (!aQueryResponse.HasError)
            {
                ipAddresses.AddRange(
                    aQueryResponse.Answers.Where(x => x is ARecord).Select(x => (x as ARecord).Address).Distinct());
            }

            if (!aaaaQueryResponse.HasError)
            {
                ipAddresses.AddRange(
                    aaaaQueryResponse.Answers.Where(x => x is AaaaRecord)
                        .Select(x => (x as AaaaRecord).Address)
                        .Distinct());
            }

            return ipAddresses;
        }

        public void Start()
        {
            this.logger.Debug("Starting range lookup service");
            this.client.JoinReceivedEvent += this.OnJoinEvent;
        }

        public void Stop()
        {
            this.logger.Debug("Stopping range lookup service");
            this.client.JoinReceivedEvent -= this.OnJoinEvent;
        }
    }
}