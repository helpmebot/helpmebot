namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Protected)]
    [CommandInvocation("idlehelpees")]
    [HelpSummary("Returns a list of helpees and their in-channel idle time, including the last-active helpers")]
    public class IdleHelpeesCommand :CommandBase
    {
        private readonly IHelpeeManagementService helpeeManagementService;
        private readonly IResponder responder;

        public IdleHelpeesCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IHelpeeManagementService helpeeManagementService,
            IResponder responder)
            : base(commandSource, user, arguments, logger, flagService, configurationProvider, client)
        {
            this.helpeeManagementService = helpeeManagementService;
            this.responder = responder;
        }

        protected override IEnumerable<CommandResponse> Execute()
        {
            var helpees = this.helpeeManagementService.Helpees
                .Select(
                    delegate(KeyValuePair<IrcUser, DateTime> x)
                    {
                        if (x.Value == DateTime.MinValue)
                        {
                            return this.responder.GetMessagePart("channelservices.command.idlehelpees.user.always", this.CommandSource, x.Key.Nickname);
                        }

                        var arguments = new object[] { x.Key.Nickname, DateTime.UtcNow - x.Value };
                        return this.responder.GetMessagePart("channelservices.command.idlehelpees.user.since", this.CommandSource, arguments);
                        
                    })
                .Aggregate(string.Empty, (cur, next) => cur + "; " + next);

            // U+200B is a zero-width space
            var lastActiveHelpers = this.helpeeManagementService.Helpers
                .OrderByDescending(x => x.Value)
                .Where(x => x.Value != DateTime.MinValue)
                .Take(3)
                .Select(x => this.responder.GetMessagePart("channelservices.command.idlehelpees.user.since", this.CommandSource, new object[] {x.Key.Nickname.Insert(1, "\u200B"), DateTime.UtcNow - x.Value}))
                .Aggregate(string.Empty, (cur, next) => cur + "; " + next);

            helpees = helpees.TrimStart(' ', ';');
            lastActiveHelpers = lastActiveHelpers.TrimStart(' ', ';');

            return this.responder.Respond(
                "channelservices.command.idlehelpees",
                this.CommandSource,
                new object[] { helpees, lastActiveHelpers });
        }
    }
}