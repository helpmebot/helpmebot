namespace Helpmebot.CategoryWatcher.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Attributes;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using CoreServices.Model;
    using CoreServices.Services.Messages.Interfaces;
    using Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;
    
    [CommandInvocation("catwatcher")]
    [CommandFlag(Flags.Protected)]
    [HelpCategory("CatWatcher")]
    [HelpSummary("Manages category watcher configuration")]
    public class CatwatcherConfigCommand : CommandBase
    {
        private readonly IWatcherConfigurationService catWatcherConfig;
        private readonly IResponder responder;

        public CatwatcherConfigCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IWatcherConfigurationService catWatcherConfig,
            IResponder responder
            ) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.catWatcherConfig = catWatcherConfig;
            this.responder = responder;
        }

        [SubcommandInvocation("list")]
        [Help("", "Lists all configured category watchers")]
        protected IEnumerable<CommandResponse> ListMode()
        {
            var responses = new List<CommandResponse>
            {
                new CommandResponse
                {
                    Message = this.responder.GetMessagePart(
                        "catwatcher.command.catwatcher.list.start",
                        this.CommandSource)
                }
            };
            
            var watchers = this.catWatcherConfig.GetWatchers()
                .Select(
                    x => this.responder.GetMessagePart(
                        "catwatcher.command.catwatcher.list.item",
                        this.CommandSource,
                        new object[] { x.Keyword, x.Category }))
                .Select(x => new CommandResponse { Message = x });
            responses.AddRange(watchers);

            return responses;
        }
    }
}