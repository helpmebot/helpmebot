namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("untrack")]
    [CommandFlag(Flags.Protected)]
    public class TrollIgnoreListCommand : CommandBase
    {
        private readonly ITrollMonitoringService trollMonitoringService;

        public TrollIgnoreListCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ITrollMonitoringService trollMonitoringService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.trollMonitoringService = trollMonitoringService;
        }

        [RequiredArguments(1)]
        [Help("<nickname>", "Prevents the provided nickname from triggering tracking alerts. This safe-listing will only last until they leave the channel, and only gives a reprieve from bot-initiated violence by reducing the user's current score by a large value. Sufficient additional hits will result in the user being added back to tracking.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            try
            {
                var score = -1000;
                var nickname = this.Arguments.First();
                var user = this.trollMonitoringService.SetScore(nickname, score);

                if (user == null)
                {
                    return new[] { new CommandResponse { Message = $"{nickname} not found on tracked channel." } };
                }
                
                return new[] { new CommandResponse { Message = $"Score of {user} set to {score}." } };
            }
            catch (Exception ex)
            {
                return new[]
                {
                    new CommandResponse
                    {
                        Message = $"Unhandled exception: {ex.Message}",
                        Destination = CommandResponseDestination.PrivateMessage
                    }
                };
            }
        }
    }
}