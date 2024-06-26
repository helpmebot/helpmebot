namespace Helpmebot.Commands.Commands.BotManagement
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Owner)]
    [CommandInvocation("quit")]
    [CommandInvocation("die")]
    [HelpSummary("Shuts down the bot.")]
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
                ((IrcClient)this.Client).Inject("QUIT :Requested by " + this.User);
                this.application.Stop();    
            }
            
            yield break;
        }
    }
}