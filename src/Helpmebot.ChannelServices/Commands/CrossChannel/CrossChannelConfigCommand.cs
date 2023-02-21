namespace Helpmebot.ChannelServices.Commands.CrossChannel
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using CoreServices.Services.Interfaces;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("crosschannel")]
    [CommandFlag(Flags.Configuration, true)]
    public class CrossChannelConfigCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;
        private readonly ICrossChannelService crossChannelService;

        public CrossChannelConfigCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ICrossChannelService crossChannelService,
            IResponder responder,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
            this.channelManagementService = channelManagementService;
            this.crossChannelService = crossChannelService;
        }

        [RequiredArguments(1)]
        [SubcommandInvocation("configure")]
        [Help("<channel>", new[]{"Sets up cross-channel notifications from the provided (\"frontend\") channel to this (\"backend\") channel.","This command should be run in the desired backend channel."})]
        protected IEnumerable<CommandResponse> ConfigureMode()
        {
            var backendName = this.CommandSource;
            var frontendName = this.Arguments.First();

            if (!this.channelManagementService.IsEnabled(backendName))
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, backendName));
            }
            
            if (!this.channelManagementService.IsEnabled(frontendName))
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, frontendName));
            }

            this.crossChannelService.Configure(frontendName, backendName);
            
            return this.responder.Respond("common.done", this.CommandSource);
        }
        
        [Help("", new[]{"Removes cross-channel notification configuration from this channel.","This command should be run in the backend channel."})]
        [SubcommandInvocation("deconfigure")]
        protected IEnumerable<CommandResponse> DeconfigureMode()
        {
            var backendName = this.CommandSource;
            
            if (!this.channelManagementService.IsEnabled(backendName))
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, backendName));
            }
            
            this.crossChannelService.Deconfigure(backendName);
            
            return this.responder.Respond("common.done", this.CommandSource);
        }
        
        [SubcommandInvocation("notifications")]
        [SubcommandInvocation("notify")]
        [Help("", "Manages the notification state. Without parameters, this will return the current state.")]
        [CommandParameter("enable", "Enable notifications", "enable", typeof(bool), hidden: true)]
        [CommandParameter("disable", "Disable notifications", "enable", typeof(bool), booleanInverse: true, hidden: true)]
        protected IEnumerable<CommandResponse> NotifyMode()
        {
            var status = this.Parameters.GetParameter("enable", (bool?)null);
            
            var backendName = this.CommandSource;
            if (!this.channelManagementService.IsEnabled(backendName))
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, backendName));
            }
            
            if (!status.HasValue)
            {
                var currentStatus = this.crossChannelService.GetNotificationStatus(backendName);
                
                return this.responder.Respond(
                    "channelservices.command.crosschannel." + (currentStatus ? "enabled" : "disabled"),
                    this.CommandSource,
                    backendName);
            }

            this.crossChannelService.SetNotificationStatus(backendName, status.Value);
                
            return this.responder.Respond("common.done", this.CommandSource);
        }
        
        [SubcommandInvocation("notifykeyword")]
        [Help("<keyword>", "Sets the keyword used for triggering the notification")]
        [RequiredArguments(1)]
        protected IEnumerable<CommandResponse> NotifyKeywordMode()
        {
            var backendName = this.CommandSource;
            
            if (!this.channelManagementService.IsEnabled(backendName))
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, backendName));
            }

            this.crossChannelService.SetNotificationKeyword(backendName, this.Arguments.First());

            return this.responder.Respond("common.done", this.CommandSource);
        }
        
        [SubcommandInvocation("notifymessage")]
        [Help("<message>", "Sets the message used for the notification.")]
        [RequiredArguments(1)]
        protected IEnumerable<CommandResponse> NotifyMessageMode()
        {
            var backendName = this.CommandSource;

            if (!this.channelManagementService.IsEnabled(backendName))
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, backendName));
            }

            this.crossChannelService.SetNotificationMessage(
                backendName,
                string.Join(" ", this.Arguments));

            return this.responder.Respond("common.done", this.CommandSource);
        }
    }
}