namespace Helpmebot.CoreServices.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using Interfaces;
    using NHibernate;

    public class JoinMessageConfigurationService : IJoinMessageConfigurationService
    {
        private readonly ISession database;
        private readonly ILogger logger;

        public JoinMessageConfigurationService(ISession database, ILogger logger)
        {
            this.database = database;
            this.logger = logger;
        }   
        
        public IEnumerable<string> GetOverridesForChannel(string channel)
        {
            return this.database.QueryOver<WelcomerOverride>()
                .Where(x => x.ChannelName == channel)
                .List().Select(x => x.ActiveFlag);
        }
    }
}