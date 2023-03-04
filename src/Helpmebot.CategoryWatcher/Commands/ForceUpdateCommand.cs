namespace Helpmebot.CategoryWatcher.Commands
{
    using System.Collections.Generic;
    using Attributes;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CategoryWatcher.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Info)]
    [ForceDocumented(promoteAliases:true)]
    [HelpSummary("Retrieves the current items in the associated category.")]
    [HelpCategory("CatWatcher")]
    public class ForceUpdateCommand : CommandBase
    {
        private readonly IForcedUpdateHelper helperService;

        public ForceUpdateCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IForcedUpdateHelper helperService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.helperService = helperService;
        }
        
        protected override IEnumerable<CommandResponse> Execute()
        {
            return this.helperService.DoForcedUpdate(this.InvokedAs, this.CommandSource);
        }
    }
}