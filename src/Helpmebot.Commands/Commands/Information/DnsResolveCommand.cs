namespace Helpmebot.Commands.Commands.Information
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using DnsClient;
    using DnsClient.Protocol;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("resolve")]
    [CommandInvocation("dns")]
    [CommandFlag(Flags.Info)]
    [HelpSummary("Performs a DNS lookup on the requested address")]
    public class DnsResolveCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly LookupClient dnsClient;

        public DnsResolveCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
            var options = new LookupClientOptions {UseCache = false, EnableAuditTrail = true};
            this.dnsClient = new LookupClient(options);
        }

        [RequiredArguments(1)]
        [Help(new[] {"<ip>", "<hostname>"})]
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
                return this.responder.Respond(
                    "commands.command.resolve.ptr.error",
                    this.CommandSource,
                    new object[] { address, ptrQueryResponse.ErrorMessage });
            }
            
            var domains = ptrQueryResponse.Answers.Select(x => (x as PtrRecord).PtrDomainName.ToString())
                .Distinct()
                .ToList();

            return this.responder.Respond(
                "commands.command.resolve.ptr",
                this.CommandSource,
                new object[] { address, string.Join(", ", domains) });
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

            var canonical = string.Empty;
            if (cnames.Any())
            {
                canonical = this.responder.GetMessagePart(
                    "commands.command.resolve.canonical",
                    this.CommandSource,
                    new object[] { string.Join(", ", cnames) });
            }
            
            if (!ipAddresses.Any())
            {
                return this.responder.Respond(
                    "commands.command.resolve.host.none",
                    this.CommandSource,
                    new object[] { query, canonical });
            }

            return this.responder.Respond(
                "commands.command.resolve.host",
                this.CommandSource,
                new object[] { query, string.Join(", ", ipAddresses), canonical });
        }
    }
}