namespace Helpmebot.ChannelServices.Commands.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.LocalConfiguration)]
    [CommandFlag(Flags.Configuration, true)]
    [CommandInvocation("silence")]
    public class SilenceCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public SilenceCommand(
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
        [Help("", "Enables silent mode for the current channel")]
        protected IEnumerable<CommandResponse> EnableCommand()
        {
            try
            {
                this.channelManagementService.ConfigureSilence(this.CommandSource, true);
                return this.responder.Respond("channelservices.command.silence.enabled", this.CommandSource);
            }
            catch (NullReferenceException)
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
            }
        }

        [SubcommandInvocation("disable")]
        [Help("", "Disables silent mode for the current channel")]
        protected IEnumerable<CommandResponse> DisableCommand()
        {
            try
            {
                this.channelManagementService.ConfigureSilence(this.CommandSource, false);
                return this.responder.Respond("channelservices.command.silence.disabled", this.CommandSource);
            }
            catch (NullReferenceException)
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
            }
        }
    }
}