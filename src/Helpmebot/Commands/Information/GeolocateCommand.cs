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
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("geolocate")]
    [CommandFlag(Flags.Protected)]
    public class GeolocateCommand : CommandBase
    {
        private readonly IGeolocationService geolocationService;

        public GeolocateCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IGeolocationService geolocationService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.geolocationService = geolocationService;
        }

        [RequiredArguments(1)]
        [Help(
            new[] {"<ip>", "<hexstring>", "<nickname>"},
            "Returns the real-world location for the provided IP address")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var ipAddress = this.GetIPAddress();
            if (ipAddress == null)
            {
                yield return new CommandResponse { Message = "Unable to find IP address from argument." };
                yield break;
            }
            
            
            var location = this.geolocationService.GetLocation(ipAddress);

            yield return new CommandResponse
            {
                Message = string.Format("Location: {0}", location)
            };
        }
    }
}