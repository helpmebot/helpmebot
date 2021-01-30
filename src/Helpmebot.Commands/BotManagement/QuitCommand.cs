namespace Helpmebot.Commands.BotManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flag.Owner)]
    [CommandInvocation("quit")]
    [CommandInvocation("die")]
    public class QuitCommand : CommandBase
    {
        private readonly IApplication application;

        public QuitCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client, IApplication application) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.application = application;
        }

        [Help("<nickname>", "Shuts down the bot. Provide the bot's nickname to confirm the request")]
        [RequiredArguments(1)]
        protected override IEnumerable<CommandResponse> Execute()
        {
            if (this.Arguments.First() == this.Client.Nickname)
            {
                this.application.Stop();    
            }
            
            yield break;
        }
    }
}