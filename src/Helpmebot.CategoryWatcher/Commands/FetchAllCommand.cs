namespace Helpmebot.CategoryWatcher.Commands
{
    using System.Collections.Generic;
    using Attributes;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using CoreServices.Services.Interfaces;
    using Helpmebot.CategoryWatcher.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("fetchall")]
    [CommandFlag(Flags.Protected)]
    [HelpCategory("CatWatcher")]
    [HelpSummary("Returns the current state of all category watchers configured in the current channel.")]
    public class FetchAllCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;
        private readonly IForcedUpdateHelper helper;

        public FetchAllCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
            IChannelManagementService channelManagementService,
            IForcedUpdateHelper helper) : base(
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
            this.helper = helper;
        }

        [CommandParameter(
            "all",
            "Return the current state of all category watchers configured on the bot.",
            "all",
            typeof(bool))]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var allKeywords = this.Parameters.GetParameter("all", false);

            if (!this.channelManagementService.IsEnabled(this.CommandSource) && !allKeywords)
            {
                return this.responder.Respond("catwatcher.command.fetchall.must-run-in-channel", this.CommandSource);
            }

            return this.helper.BulkForcedUpdate(allKeywords, this.CommandSource);
        }
    }
}