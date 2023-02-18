namespace Helpmebot.CoreServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
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
                .List()
                .Select(x => x.ActiveFlag);
        }

        public IList<WelcomeUser> GetExceptions(string channel)
        {
            var exceptions = this.database.QueryOver<WelcomeUser>()
                .Where(x => x.Channel == channel && x.Exception)
                .List();

            return exceptions;
        }

        public IList<WelcomeUser> GetUsers(string channel)
        {
            var exceptions = this.database.QueryOver<WelcomeUser>()
                .Where(x => x.Channel == channel && !x.Exception)
                .List();

            return exceptions;
        }

        public WelcomerOverride GetOverride(string channel, string welcomerFlag)
        {
            var overrides = this.database.QueryOver<WelcomerOverride>()
                .Where(x => x.ActiveFlag == welcomerFlag)
                .And(x => x.ChannelName == channel)
                .List();

            return overrides.FirstOrDefault();
        }

        public void AddWelcomeEntry(WelcomeUser welcomeUser)
        {
            this.database.BeginTransaction(IsolationLevel.RepeatableRead);
            try
            {
                this.database.Save(welcomeUser);
                this.database.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                this.database.GetCurrentTransaction().Rollback();
                this.logger.Error("Error occurred during addition of welcome mask.", e);
                throw;
            }
        }

        public void RemoveWelcomeEntry(WelcomeUser user)
        {
            this.database.BeginTransaction(IsolationLevel.RepeatableRead);
            try
            {
                this.database.Delete(user);
                this.database.GetCurrentTransaction().Commit();
            }
            catch (Exception e)
            {
                this.database.GetCurrentTransaction().Rollback();
                this.logger.Error("Error occurred during removal of welcome mask.", e);
                throw;
            }
        }
    }
}