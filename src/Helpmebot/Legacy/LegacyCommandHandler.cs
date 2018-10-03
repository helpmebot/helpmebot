namespace Helpmebot.Legacy
{
    using System;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Legacy.Transitional;
    using Helpmebot.Services.Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;

    public class LegacyCommandHandler : ILegacyCommandHandler
    {
        private readonly ILogger logger;
        
        private readonly IRedirectionParserService redirectionParserService;
        private readonly BotConfiguration botConfiguration;
        private readonly ICategoryWatcherHelperService categoryWatcherHelperService;
        private readonly ILegacyAccessService legacyAccessService;

        public LegacyCommandHandler(ILogger logger,
            IRedirectionParserService redirectionParserService,
            BotConfiguration botConfiguration,
            ICategoryWatcherHelperService categoryWatcherHelperService,
            ILegacyAccessService legacyAccessService)
        {
            this.logger = logger;

            this.redirectionParserService = redirectionParserService;
            this.botConfiguration = botConfiguration;
            this.categoryWatcherHelperService = categoryWatcherHelperService;
            this.legacyAccessService = legacyAccessService;
        }

        public void ReceivedMessage(object sender, MessageReceivedEventArgs e)
        {
            if (e.IsNotice)
            {
                return;
            }

            string message = e.Message;

            var cmd = new LegacyCommandParser(
                ServiceLocator.Current.GetInstance<ICommandServiceHelper>(),
                this.logger.CreateChildLogger("LegacyCommandParser"),
                this.redirectionParserService,
                this.botConfiguration,
                this.legacyAccessService);
            try
            {
                bool overrideSilence = cmd.OverrideBotSilence;
                if (cmd.IsRecognisedMessage(ref message, ref overrideSilence, (IIrcClient)sender))
                {
                    cmd.OverrideBotSilence = overrideSilence;
                    string[] messageWords = message.Split(' ');
                    string command = messageWords[0].ToLowerInvariant();
                    string joinedargs = string.Join(" ", messageWords, 1, messageWords.Length - 1);
                    string[] commandArgs = joinedargs == string.Empty ? new string[0] : joinedargs.Split(' ');

                    cmd.HandleCommand(e.User, e.Target, command, commandArgs);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.Message, ex);
            }
        }
    }
}