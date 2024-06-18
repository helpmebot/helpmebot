namespace Helpmebot.ChannelServices.Commands.ChannelManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Messages;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Owner)]
    [CommandInvocation("kb")]
    [HelpSummary("Kickbans the specified user.")]
    public class BanCommand : CommandBase
    {
        private readonly IModeMonitoringService modeMonitoringService;

        public BanCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IModeMonitoringService modeMonitoringService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.modeMonitoringService = modeMonitoringService;
        }

        [RequiredArguments(1)]
        [Help("<nickname>")]
        [CommandParameter("force", "Force the kickban to apply even if the user is cloaked", "force", typeof(bool))]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var force = this.Parameters.GetParameter("force", false);
            
            try
            {
                var channel = "#wikipedia-en-help";
                var bantracker = "litharge";
                
                var banTarget = this.Client.Channels[channel]
                    .Users.First(x => x.Value.User.Nickname == this.Arguments[0]);

                if (banTarget.Value.User.Hostname.Contains("/") && !banTarget.Value.User.Hostname.StartsWith("user/") && !force)
                {
                    this.Client.SendMessage(
                        this.User.Nickname,
                        $"{banTarget} has a project cloak. Are you sure? Use --force to force this kickban through.");
                    return null;
                }

                this.modeMonitoringService.PerformAsOperator(
                    channel,
                    ircClient =>
                    {
                        ircClient.Mode(channel, $"+b *!*@{banTarget.Value.User.Hostname}");
                        ircClient.Send(new Message("KICK", new[] {channel, banTarget.Value.User.Nickname}));
                        
                        // allow time for eir to message us
                        Thread.Sleep(1000);
                        ircClient.SendMessage(bantracker, $"1d Requested by {this.User} via manual request");
                    });
                
                this.Client.SendMessage("#wikipedia-en-helpers", $"{banTarget.Value.User.Nickname} banned by request of {this.User}");
            }
            catch (Exception ex)
            {
                return new[] {new CommandResponse {Message = ex.Message}};
            }

            return null;
        }
    }
}