namespace Helpmebot.Legacy.Transitional
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Microsoft.Practices.ServiceLocation;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class LegacyFlagService : IFlagService
    {
        private readonly ISession session;
        private readonly ILogger logger;

        public LegacyFlagService(ISession session, ILogger logger)
        {
            this.session = session;
            this.logger = logger;
        }

        private LegacyUserRights GetLegacyUserRights(IUser user, IIrcClient client)
        {
            try
            {
                var users = this.session.CreateCriteria<User>().List<User>();

                users = users.Where(x => new IrcUserMask(x.Mask, client).Matches(user).GetValueOrDefault()).ToList();

                foreach (var level in new[] {"Developer", "Superuser", "Ignored", "Semiignored", "Advanced", "Normal"})
                {
                    if (users.Any(x => x.AccessLevel == level))
                    {
                        return (LegacyUserRights) Enum.Parse(typeof(LegacyUserRights), level);
                    }
                }

                return LegacyUserRights.Normal;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.Message, ex);
            }

            return LegacyUserRights.Normal;
        }

        public bool UserHasFlag(IUser user, string flag, string locality)
        {
            return this.GetFlagsForUser(user, locality).Contains(flag);
        }

        public IEnumerable<string> GetFlagsForUser(IUser user, string locality)
        {
            var legacyUserRights = this.GetLegacyUserRights(user, ServiceLocator.Current.GetInstance<IIrcClient>());

            var group = this.session.CreateCriteria<FlagGroup>()
                .Add(Restrictions.Eq("Name", legacyUserRights.ToString()))
                .UniqueResult<FlagGroup>();

            if (group == null)
            {
                return new string[0];
            }

            var flagsForUser = group.Flags.ToCharArray()
                .Where(x => x != '+' && x != '-' && x != '*')
                .Select(x => new string(new[] {x}));

            return flagsForUser;
        }
    }
}