namespace Helpmebot.CoreServices.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;

    public class DatabaseLegacyMessageBackend :ILegacyMessageBackend
    {
        private readonly ISession localSession;

        public DatabaseLegacyMessageBackend(ISession localSession)
        {
            this.localSession = localSession;
        }
        
        public IEnumerable<string> GetRawMessages(string legacyKey)
        {
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