namespace Helpmebot.ChannelServices.Commands.Configuration
{
    using System;
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.LocalConfiguration)]
    [CommandFlag(Flags.Configuration, true)]
    [CommandInvocation("basewiki")]
    public class BaseWikiCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public BaseWikiCommand(
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

        [Help("", "Retrieves the base wiki this channel is configured to use")]
        [CommandParameter("target=", "The target channel to apply this command to", "target", typeof(string))]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var target = this.Parameters.GetParameter("target", this.CommandSource);
            
            try
            {
                var mediaWikiSite = this.channelManagementService.GetBaseWiki(target);

                return this.responder.Respond(
                    "channelservices.command.basewiki",
                    this.CommandSource,
                    new object[] { target, mediaWikiSite.WikiId, mediaWikiSite.Api });
            }
            catch (NullReferenceException)
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, target));
            }
        }
    }
}