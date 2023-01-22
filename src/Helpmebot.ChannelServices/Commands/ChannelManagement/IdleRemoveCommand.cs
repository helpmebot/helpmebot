namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.CoreServices.Startup;
    using NDesk.Options;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Messages;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.ChanOp)]
    [CommandInvocation("idleremove")]
    public class IdleRemoveCommand : CommandBase
    {
        private readonly IHelpeeManagementService helpeeManagementService;
        private readonly IModeMonitoringService modeMonitoringService;
        private readonly IResponder responder;

        public IdleRemoveCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IHelpeeManagementService helpeeManagementService,
            IModeMonitoringService modeMonitoringService,
            IResponder responder)
            : base(commandSource, user, arguments, logger, flagService, configurationProvider, client)
        {
            this.helpeeManagementService = helpeeManagementService;
            this.modeMonitoringService = modeMonitoringService;
            this.responder = responder;
        }

        [Help("[nickname...]", "Produces a list of who will be removed from the channel by the remove command")]
        [SubcommandInvocation("dryrun")]
        protected IEnumerable<CommandResponse> DryRun()
        {
            var force = false;
            var opts = new OptionSet
            {
                { "force", x => force = true },
            };

            var remainingArgs = opts.Parse(this.Arguments);

            var removableHelpees = this.GetRemovableHelpees(remainingArgs, force);

            var helpees = string.Join(", ", removableHelpees);

            return this.responder.Respond("channelservices.command.idlehelpees.remove", this.CommandSource, helpees);
        }

        [Help("[nickname...]", "Removes the listed helpees who have been inactive for more than 15 minutes by the bot's reckoning. Nicknames not eligible are automatically filtered out, but please use the dryrun subcommand to check prior to using this.")]
        [SubcommandInvocation("remove")]
        protected IEnumerable<CommandResponse> Remove()
        {
            var force = false;
            var opts = new OptionSet
            {
                { "force", x => force = true },
            };

            var remainingArgs = opts.Parse(this.Arguments);

            var removableHelpees = this.GetRemovableHelpees(remainingArgs, force);

            var channel = "#wikipedia-en-help";
            var removeMessage = this.responder.GetMessagePart("channelservices.command.idlehelpees.kick", this.CommandSource);

            this.modeMonitoringService.PerformAsOperator(
                channel,
                ircClient =>
                {
                    foreach (var helpee in removableHelpees)
                    {
                        ircClient.Send(new Message("KICK", new[] { channel, helpee, removeMessage }));
                    }
                });

            var helpees = string.Join(", ", removableHelpees);
            if (this.CommandSource == "#wikipedia-en-helpers")
            {
                this.Client.SendMessage(
                    "#wikipedia-en-helpers",
                    this.responder.GetMessagePart(
                        "channelservices.command.idlehelpees.local-confirm",
                        this.CommandSource,
                        helpees));
            }
            else
            {
                this.Client.SendMessage(
                    "#wikipedia-en-helpers",
                    this.responder.GetMessagePart(
                        "channelservices.command.idlehelpees.confirm",
                        this.CommandSource,
                        new object[] { helpees, this.User.Nickname }));
            }

            return null;
        }

        private List<string> GetRemovableHelpees(IList<string> helpees, bool force)
        {
            if (!this.FlagService.UserHasFlag(this.User, Flags.Owner, this.CommandSource) && force)
            {
                throw new CommandAccessDeniedException();
            }

            if (force)
            {
                return this.Client.Channels[this.helpeeManagementService.TargetChannel]
                    .Users.Select(x => x.Key)
                    .Intersect(helpees)
                    .Except(new List<string> { "ChanServ", this.Client.Nickname })
                    .ToList();
            }

            var removableHelpees = this.helpeeManagementService.Helpees
                .Where(
                    x =>
                    {
                        if (x.Value == DateTime.MinValue)
                        {
                            if ((DateTime.UtcNow - Launcher.StartupTime).TotalMinutes > 15)
                            {
                                return true;
                            }
                        }

                        if ((DateTime.UtcNow - x.Value).TotalMinutes > 15)
                        {
                            return true;
                        }

                        return false;
                    })
                .Select(x => x.Key.Nickname)
                .Intersect(helpees)
                .ToList();
            return removableHelpees;
        }
    }
}