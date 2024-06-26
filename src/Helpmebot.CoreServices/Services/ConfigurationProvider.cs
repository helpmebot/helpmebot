namespace Helpmebot.CoreServices.Services
{
    using Helpmebot.Configuration;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;

    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly BotConfiguration botConfig;

        public ConfigurationProvider(BotConfiguration botConfig)
        {
            this.botConfig = botConfig;
        }
        
        public string CommandPrefix
        {
            get { return this.botConfig.CommandTrigger; }
        }
        
        public string DebugChannel
        {
            get { return this.botConfig.DebugChannel; }
        }

        public bool AllowQuotedStrings { get; } = false;
        public bool IncludeBuiltins => true;
        public string UseCommandInitSeparator => null;
    }
}