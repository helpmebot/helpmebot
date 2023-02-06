namespace Helpmebot.CoreServices.ExtensionMethods
{
    using Configuration;
    using Helpmebot.Model;
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

        public static MediaWikiSite ToDatabaseType(this MediaWikiSiteConfiguration.MediaWikiSite config)
        {
            return new MediaWikiSite
            {
                Api = config.Api,
                IsDefault = false,
                Username = config.Credentials.Username,
                Password = config.Credentials.Password,
                WikiId = config.WikiId
            };
        }

        public static MediaWikiSiteConfiguration.MediaWikiSite ToConfigurationType(this MediaWikiSite config)
        {
            return new MediaWikiSiteConfiguration.MediaWikiSite
            {
                Api = config.Api,
                Credentials = new MediaWikiSiteConfiguration.MediaWikiCredentials
                {
                    Username = config.Username,
                    Password = config.Password
                },
                WikiId = config.WikiId
            };
        }
    }
}