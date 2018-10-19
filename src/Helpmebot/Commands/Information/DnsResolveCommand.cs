namespace Helpmebot.Commands.Information
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Castle.Core.Logging;
    using DnsClient;
    using DnsClient.Protocol;
    using Helpmebot.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("resolve")]
    [CommandInvocation("dns")]
    [CommandFlag(Flags.Info)]
    public class DnsResolveCommand : CommandBase
    {
        private readonly LookupClient dnsClient;

        public DnsResolveCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.dnsClient = new LookupClient {UseCache = false, EnableAuditTrail = true};
        }

        [RequiredArguments(1)]
        [Help(new[] {"<ip>", "<hostname>"}, "Performs a DNS lookup on the requested address")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var query = this.Arguments.First();

            IPAddress address;
            if (IPAddress.TryParse(query, out address))
            {
                return this.HandlePtr(address);
            }

            return this.HandleHost(query);
        }

        private IEnumerable<CommandResponse> HandlePtr(IPAddress address)
        {
            var ptrQueryResponse = this.dnsClient.QueryReverse(address);

            if (ptrQueryResponse.HasError)
            {
                yield return new CommandResponse
                {
                    Message = string.Format(
                        "Could not look up hostname for IP {0}: {1}",
                        address,
                        ptrQueryResponse.ErrorMessage)
                };
                yield break;
            }

            var domains = ptrQueryResponse.Answers.Select(x => (x as PtrRecord).PtrDomainName.ToString())
                .Distinct()
                .ToList();

            yield return new CommandResponse
            {
                Message = string.Format(
                    "{0} resolves to: {1}",
                    address,
                    string.Join(", ", domains))
            };
        }

        private IEnumerable<CommandResponse> HandleHost(string query)
        {
            var aQueryResponse = this.dnsClient.Query(query, QueryType.A);
            var aaaaQueryResponse = this.dnsClient.Query(query, QueryType.AAAA);
            var cnameQueryResponse = this.dnsClient.Query(query, QueryType.CNAME);

            var cnames = aaaaQueryResponse.Answers.Where(x => x is CNameRecord)
                .Union(aQueryResponse.Answers.Where(x => x is CNameRecord))
                .Union(cnameQueryResponse.Answers.Where(x => x is CNameRecord))
                .Select(x => (x as CNameRecord).CanonicalName.ToString())
                .Distinct()
                .ToList();

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

            if (!ipAddresses.Any())
            {
                var canonical = string.Empty;
                if (cnames.Any())
                {
                    canonical = string.Format(" (canonical: {0})", string.Join(", ", cnames));
                }

                yield return new CommandResponse
                {
                    Message = string.Format("No IP addresses found for hostname {0}{1}.", query, canonical)
                };
                yield break;
            }

            var ipMessage = string.Format(
                "{0} {2}resolves to the following IP addresses: {1}",
                query,
                string.Join(", ", ipAddresses),
                cnames.Count > 0
                    ? string.Format("(canonical: {0}) ", string.Join(", ", cnames))
                    : string.Empty
            );

            yield return new CommandResponse {Message = ipMessage};
        }
    }
}