namespace Helpmebot.Commands.Information
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
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
    }
}