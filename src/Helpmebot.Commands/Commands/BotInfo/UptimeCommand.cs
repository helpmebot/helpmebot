namespace Helpmebot.Commands.Commands.BotInfo
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.CoreServices.Startup;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("uptime")]
    [CommandFlag(Flags.BotInfo)]
    public class UptimeCommand : CommandBase
    {
        private readonly IMessageService messageService;

        public UptimeCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMessageService messageService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.messageService = messageService;
        }

        [Help("", "Returns the current uptime of the bot")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var startupTime = Launcher.StartupTime;
            
            string[] messageParams =
            {
                startupTime.DayOfWeek.ToString(), 
                startupTime.ToLongDateString(), 
                startupTime.ToLongTimeString()
            };

            var message = this.messageService.RetrieveMessage("cmdUptimeUpSince", this.CommandSource, messageParams);
            yield return new CommandResponse {Message = message};
        }
    }
}