namespace Helpmebot.CoreServices.Services.Messages
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;

    public class DatabaseLegacyMessageBackend :ILegacyMessageBackend
    {
        private readonly ISession localSession;
        private readonly ILogger logger;

        public DatabaseLegacyMessageBackend(ISession localSession, ILogger logger)
        {
            this.localSession = localSession;
            this.logger = logger;
        }
        
        public IEnumerable<string> GetRawMessages(string legacyKey)
        {
            this.logger.Debug($"Getting messages from database for {legacyKey}");
            Response response;
            lock (this.localSession)
            {
                response = this.localSession.CreateCriteria<Response>()
                    .Add(Restrictions.Eq("Name", Encoding.UTF8.GetBytes(legacyKey)))
                    .UniqueResult<Response>();
            }
            
            if (response != null)
            {
                // extract the byte array from the dataset
                string text = Encoding.UTF8.GetString(response.Text);
                return text.Split('\n').ToList();
            }

            return new List<string>();
        }
        
        public void RefreshResponseRepository()
        {
            lock (this.localSession)
            {
                var all = this.localSession.CreateCriteria<Response>().List<Response>();
                foreach (var model in all)
                {
                    this.localSession.Refresh(model);
                }
            }
        }
    }
}