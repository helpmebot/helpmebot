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
    using Helpmebot.CoreServices.Startup;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Messages;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.ChanOp)]
    [CommandInvocation("idleremove")]
    [HelpSummary("Removes idle helpees from the channel")]
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

        [CommandParameter("force", "Force the removal, bypassing safety protections", "force", typeof(bool), true)]
        [CommandParameter("dry-run", "List who will be removed from the channel, but don't actually do it", "dryrun", typeof(bool))]
        [Help("<nickname...>", "Removes the listed helpees who have been inactive for more than 15 minutes by the bot's reckoning. Nicknames not eligible are automatically filtered out.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var force = this.Parameters.GetParameter("force", false);
            var dryRun = this.Parameters.GetParameter("dryrun", false);
            
            var removableHelpees = this.GetRemovableHelpees(this.Arguments, force);

            if (removableHelpees.Count == 0)
            {
                return this.responder.Respond(
                    "channelservices.command.idlehelpees.no-eligible",
                    this.CommandSource);
            }

            if (dryRun)
            {
                var helpees = string.Join(", ", removableHelpees);
                return this.responder.Respond(
                    "channelservices.command.idlehelpees.remove",
                    this.CommandSource,
                    helpees);
            }

            this.DoRemoval(removableHelpees);
            return null;
        }

        [Help("<nickname...>", "Produces a list of who will be removed from the channel by the remove command")]
        [SubcommandInvocation("dryrun")]
        [CommandParameter("force", "Force the removal, bypassing safety protections", "force", typeof(bool), true)]
        [Undocumented]
        protected IEnumerable<CommandResponse> DryRun()
        {
            var force = this.Parameters.GetParameter("force", false);

            var removableHelpees = this.GetRemovableHelpees(this.Arguments, force);

            if (removableHelpees.Count == 0)
            {
                return this.responder.Respond(
                    "channelservices.command.idlehelpees.no-eligible",
                    this.CommandSource);
            }

            var helpees = string.Join(", ", removableHelpees);

            return this.responder.Respond("channelservices.command.idlehelpees.remove", this.CommandSource, helpees);
        }

        [Help("<nickname...>", "Removes the helpees in the provided list who have been inactive for more than 15 minutes by the bot's reckoning. Nicknames not eligible are automatically filtered out, but please use the dryrun subcommand to check prior to using this.")]
        [SubcommandInvocation("remove")]
        [CommandParameter("force", "Force the removal, bypassing safety protections", "force", typeof(bool), true)]
        [Undocumented]
        protected IEnumerable<CommandResponse> Remove()
        {
            var force = this.Parameters.GetParameter("force", false);

            var removableHelpees = this.GetRemovableHelpees(this.Arguments, force);

            if (removableHelpees.Count == 0)
            {
                this.Client.SendMessage(
                    "#wikipedia-en-helpers",
                    this.responder.GetMessagePart(
                        "channelservices.command.idlehelpees.no-eligible",
                        this.CommandSource));
                return null;
            }
            
            this.DoRemoval(removableHelpees);

            return null;
        }

        private void DoRemoval(List<string> removableHelpees)
        {
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