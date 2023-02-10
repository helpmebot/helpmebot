namespace Helpmebot.ChannelServices.Commands.Configuration
{
    using System;
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.LocalConfiguration)]
    [CommandFlag(Flags.Configuration, true)]
    [CommandInvocation("autolink")]
    [HelpSummary("Manages in-channel automatic wikilink expansion.")]
    public class AutoLinkCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public AutoLinkCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
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
        }

        [SubcommandInvocation("enable")]
        [Help("", "Enables autolinking for the current channel")]
        protected IEnumerable<CommandResponse> EnableCommand()
        {
            try
            {
                this.channelManagementService.ConfigureAutolink(this.CommandSource, true);
                return this.responder.Respond("channelservices.command.autolink.enabled", this.CommandSource);
            }
            catch (NullReferenceException)
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
            }
        }

        [SubcommandInvocation("disable")]
        [Help("", "Disables autolinking for the current channel")]
        protected IEnumerable<CommandResponse> DisableCommand()
        {
            try
            {
                this.channelManagementService.ConfigureAutolink(this.CommandSource, false);
                return this.responder.Respond("channelservices.command.autolink.disabled", this.CommandSource);
            }
            catch (NullReferenceException)
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
            }
        }
    }
}