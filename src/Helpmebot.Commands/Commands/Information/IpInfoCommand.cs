namespace Helpmebot.Commands.Commands.Information
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Commands.ExtensionMethods;
    using Helpmebot.Commands.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("ipinfo")]
    [CommandInvocation("geolocate")]
    [CommandInvocation("whois")]
    [CommandFlag(Flags.Protected)]
    public class IpInfoCommand : CommandBase
    {
        private readonly IWhoisService whoisService;
        private readonly IGeolocationService geolocationService;

        public IpInfoCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IWhoisService whoisService,
            IGeolocationService geolocationService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.whoisService = whoisService;
            this.geolocationService = geolocationService;
        }
        
        [RequiredArguments(1)]
        [Help(
            new[] {"<ip>", "<hexstring>", "<nickname>"},
            "Returns the controlling organisation and the real-world location for the provided IP address")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var ip = this.GetIPAddress();
            if (ip == null)
            {
                throw new CommandInvocationException("Unable to find IP address to query");
            }

            var orgName = this.whoisService.GetOrganisationName(ip);
            var location = this.geolocationService.GetLocation(ip);

            if (orgName == null)
            {
                yield return new CommandResponse {Message = $"Whois failed for {ip}; Location: {location}"};
            }
            else
            {
                yield return new CommandResponse {Message = $"Whois for {ip} gives organisation {orgName}; Location: {location}"};
            }
        }
    }
}