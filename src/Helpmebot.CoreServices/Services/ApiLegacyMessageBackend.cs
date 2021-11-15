namespace Helpmebot.CoreServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Services.Interfaces;

    public class ApiLegacyMessageBackend : ILegacyMessageBackend
    {
        private readonly MediaWikiDocumentationConfiguration config;
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly ILogger logger;

        public ApiLegacyMessageBackend(MediaWikiDocumentationConfiguration config, IMediaWikiApiHelper apiHelper, ILogger logger)
        {
            this.config = config;
            this.apiHelper = apiHelper;
            this.logger = logger;
        }
        
        public IEnumerable<string> GetRawMessages(string legacyKey)
        {
            this.logger.Debug($"Getting messages from API for {legacyKey}");
            
            var messages = new List<string>();
            
            var mediaWikiApi = this.apiHelper.GetApi(
                this.config.MediaWikiApiEndpoint,
                this.config.MediaWikiApiUsername,
                this.config.MediaWikiApiPassword);

            try
            {
                var pageContent = mediaWikiApi.GetPageContent($"Message:{legacyKey}", out var _);
                if (pageContent == null)
                {
                    return messages;
                }

                messages = pageContent.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch (Exception ex)
            {
                this.logger.Error("Error encountered retrieving messages from API.", ex);
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);    
            }

            return messages;
        }

        public void RefreshResponseRepository()
        {
            // no-op
        }
    }
}