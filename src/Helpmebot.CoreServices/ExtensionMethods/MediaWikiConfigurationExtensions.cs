namespace Helpmebot.CoreServices.ExtensionMethods
{
    using Configuration;
    using Stwalkerster.Bot.MediaWikiLib.Configuration;

    public static class MediaWikiConfigurationExtensions
    {
        
        public static MediaWikiConfiguration ToMediaWikiConfiguration(this MediaWikiSiteConfiguration.MediaWikiSite config, string userAgent)
        {
            return new MediaWikiConfiguration(
                config.Api,
                userAgent,
                config.Credentials.Username,
                config.Credentials.Password);
        }
    }
}