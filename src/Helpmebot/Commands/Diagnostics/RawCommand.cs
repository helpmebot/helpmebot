namespace Helpmebot.Commands.Diagnostics
{
    using System.Collections.Generic;

    using Castle.Core.Logging;

    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;
    
    
    [CommandFlag(Flag.Owner)]
    [CommandInvocation("raw")]
    public class RawCommand : CommandBase
    {
        public RawCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client) : base(commandSource, user, arguments, logger, flagService, configurationProvider,
            client)
        {
        }

        [Help("<IRC protocol message>", "Injects the specified message directly onto the IRC network socket")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var client = (IrcClient) this.Client;

            client.Inject(this.OriginalArguments);

            yield break;
        }
    }
}