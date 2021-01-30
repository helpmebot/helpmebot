namespace Helpmebot.Commands.BotInfo
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Startup;
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
        private readonly IApplication application;

        public UptimeCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMessageService messageService,
            IApplication application) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.messageService = messageService;
            this.application = application;
        }

        [Help("", "Returns the current uptime of the bot")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var startupTime = ((Launch)this.application).StartupTime;
            
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