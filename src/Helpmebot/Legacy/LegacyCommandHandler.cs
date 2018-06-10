﻿namespace Helpmebot.Legacy
{
    using System;
    using Castle.Core.Logging;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;

    public class LegacyCommandHandler : ILegacyCommandHandler
    {
        private readonly ILogger logger;
        
        private readonly IRedirectionParserService redirectionParserService;
        private readonly BotConfiguration botConfiguration;

        public LegacyCommandHandler(ILogger logger,
            IRedirectionParserService redirectionParserService,
            BotConfiguration botConfiguration)
        {
            this.logger = logger;

            this.redirectionParserService = redirectionParserService;
            this.botConfiguration = botConfiguration;
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
                this.botConfiguration);
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

                    cmd.HandleCommand(LegacyUser.NewFromOtherUser(e.User), e.Target, command, commandArgs);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.Message, ex);
            }
        }
    }
}