namespace Helpmebot.CategoryWatcher.Commands
{
    using System.Collections.Generic;
    using Attributes;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using CoreServices.Services.Interfaces;
    using Helpmebot.CategoryWatcher.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Info)]
    [ForceDocumented(promoteAliases:true)]
    [HelpSummary("Retrieves the current items in the associated category.")]
    [HelpCategory("CatWatcher")]
    public class ForceUpdateCommand : CommandBase
    {
        private readonly ICategoryWatcherBackgroundService categoryWatcherService;
        private readonly IChannelManagementService channelManagementService;

        public ForceUpdateCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ICategoryWatcherBackgroundService categoryWatcherService,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.categoryWatcherService = categoryWatcherService;
            this.channelManagementService = channelManagementService;
        }
        
        protected override IEnumerable<CommandResponse> Execute()
        {
            if (!this.channelManagementService.IsEnabled(this.CommandSource))
            {
                throw new CommandErrorException("Could not retrieve channel configuration.");
            }
            
            this.categoryWatcherService.ForceUpdate(this.InvokedAs, this.CommandSource);
            
            return null;
        }
    }
}