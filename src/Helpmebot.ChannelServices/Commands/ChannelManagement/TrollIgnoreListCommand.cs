namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("untrack")]
    [CommandFlag(Flags.Protected)]
    [HelpSummary("Prevents the provided nickname from triggering tracking alerts. This safe-listing will only last until they leave the channel, and only gives a reprieve from bot-initiated violence by reducing the user's current score by a large value. Sufficient additional hits will result in the user being added back to tracking.")]
    public class TrollIgnoreListCommand : CommandBase
    {
        private readonly ITrollMonitoringService trollMonitoringService;
        private readonly IResponder responder;

        public TrollIgnoreListCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ITrollMonitoringService trollMonitoringService,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.trollMonitoringService = trollMonitoringService;
            this.responder = responder;
        }

        [RequiredArguments(1)]
        [Help("<nickname>")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            try
            {
                var score = -1000;
                var nickname = this.Arguments.First();
                var user = this.trollMonitoringService.SetScore(nickname, score, false);

                if (user == null)
                {
                    return this.responder.Respond("channelservices.command.track.not-found", this.CommandSource, nickname);
                }
                
                return this.responder.Respond("channelservices.command.track", this.CommandSource, new object[]{ user, score });
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