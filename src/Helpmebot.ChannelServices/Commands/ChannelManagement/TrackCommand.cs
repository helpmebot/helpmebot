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

    [CommandInvocation("track")]
    [CommandFlag(Flags.Owner)]
    public class TrackCommand : CommandBase
    {
        private readonly ITrollMonitoringService trollMonitoringService;

        public TrackCommand(
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
        [Help("<nickname>", "Adds a user to tracking")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            try
            {
                var score = 0;
                var nickname = this.Arguments.First();
                var user = this.trollMonitoringService.SetScore(nickname, score, true);

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