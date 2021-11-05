namespace Helpmebot.ChannelServices.Services
{
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.Configuration;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public class WhoisService : IWhoisService
    {
        private readonly IWebServiceClient wsc;
        private readonly BotConfiguration botConfig;

        public WhoisService(IWebServiceClient wsc, BotConfiguration botConfig)
        {
            this.wsc = wsc;
            this.botConfig = botConfig;
        }
        
        public string GetOrganisationName(IPAddress ip)
        {
            var apiResult = this.wsc.DoApiCall(
                new NameValueCollection
                {
                    {
                        "fields", "org,as,status"
                    }
                },
                string.Format("http://ip-api.com/line/{0}", ip),
                this.botConfig.UserAgent);

            var textResult = new StreamReader(apiResult).ReadToEnd();
            var resultData = textResult.Split('\r', '\n');
            if (resultData.FirstOrDefault() == "success")
            {
                return resultData[1];
            }

            return null;
        }
    }
}