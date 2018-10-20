namespace Helpmebot.Commands.Information
{
    using System.Collections.Generic;
    using System.Net;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using helpmebot6.Commands;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate.Util;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("whois")]
    [CommandFlag(Flags.Protected)]
    public class WhoisCommand : CommandBase
    {
        private readonly IWhoisService whoisService;

        public WhoisCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IWhoisService whoisService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.whoisService = whoisService;
        }

        [RequiredArguments(1)]
        [Help(
            new[] {"<ip>", "<hexstring>", "<nickname>"},
            "Returns the controlling organisation for the provided IP address")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var ip = this.GetIPAddress();
            if (ip == null)
            {
                throw new CommandInvocationException("Unable to find IP address to query");
            }

            var orgName = this.whoisService.GetOrganisationName(ip);

            if (orgName == null)
            {
                throw new CommandErrorException(string.Format("Whois for {0} failed.", ip));
            }

            var msg = string.Format("Whois for {0} gives organisation {1}", ip, orgName);
            yield return new CommandResponse {Message = msg};
        }

        private IPAddress GetIPAddress()
        {
            IPAddress ip;
            if (IPAddress.TryParse(this.Arguments[0], out ip))
            {
                return ip;
            }

            var match = Regex.Match(this.Arguments[0], "^[a-fA-F0-9]{8}$");
            if (match.Success)
            {
                // We've got a hex-encoded IP.
                return Decode.GetIpAddressFromHex(this.Arguments[0]);
            }

            IrcUser ircUser;
            if (this.Client.UserCache.TryGetValue(this.Arguments[0], out ircUser))
            {
                var userMatch = Regex.Match(ircUser.Username, "^[a-fA-F0-9]{8}$");
                if (userMatch.Success)
                {
                    // We've got a hex-encoded IP.
                    return Decode.GetIpAddressFromHex(ircUser.Username);
                }

                if (!ircUser.Hostname.Contains("/"))
                {
                    // real hostname, not a cloak
                    IPAddress[] hostAddresses = Dns.GetHostAddresses(ircUser.Hostname);
                    if (hostAddresses.Length > 0)
                    {
                        return hostAddresses.First() as IPAddress;
                    }
                }
            }

            return null;
        }
    }
}