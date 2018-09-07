namespace Helpmebot.Commands
{
    using System;
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Model;
    using Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;
    
    [CommandInvocation("time")]
    [CommandInvocation("date")]
    [CommandFlag(Flags.Info)]
    public class TimeCommand : CommandBase
    {
        private readonly IMessageService messageService;

        public TimeCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMessageService messageService) : base(
            commandSource, user, arguments, logger, flagService, configurationProvider, client)
        {
            this.messageService = messageService;
        }

        [Help("", "Returns the current UTC date and time")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            string[] messageParams =
            {
                this.User.Nickname, DateTime.Now.DayOfWeek.ToString(), 
                DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString("00"), 
                DateTime.Now.Day.ToString("00"), DateTime.Now.Hour.ToString("00"), 
                DateTime.Now.Minute.ToString("00"), DateTime.Now.Second.ToString("00")
            };
            
            string message = this.messageService.RetrieveMessage(
                "cmdTime", 
                this.CommandSource, 
                messageParams);

            yield return new CommandResponse {Message = message};
        }
    }
}