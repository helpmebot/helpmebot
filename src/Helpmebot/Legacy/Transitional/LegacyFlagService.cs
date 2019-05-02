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
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class LegacyFlagService : IFlagService
    {
        private readonly ISession session;
        private readonly ILogger logger;

        private readonly Dictionary<LegacyUserRights, HashSet<string>> flagMapping =
            new Dictionary<LegacyUserRights, HashSet<string>>
            {
                {
                    LegacyUserRights.Developer,
                    new HashSet<string> {Flag.Owner}
                },
                {
                    LegacyUserRights.Superuser,
                    new HashSet<string>
                    {
                        Flags.AccessControl, Flags.BotManagement, Flags.Brain,
                        Flags.Configuration, Flags.Uncurl
                    }
                },
                {
                    LegacyUserRights.Advanced,
                    new HashSet<string>
                        {Flags.Acc, Flags.Fun, Flags.LocalConfiguration, Flags.Protected}
                },
                {
                    LegacyUserRights.Normal,
                    new HashSet<string> {Flag.Standard, Flags.BotInfo, Flags.Info}
                },
                {
                    LegacyUserRights.Semiignored,
                    new HashSet<string> {}
                },
            };

        public LegacyFlagService(ISession session, ILogger logger)
        {
            this.session = session;
            this.logger = logger;

            foreach (var processing in this.flagMapping)
            {
                foreach (var search in this.flagMapping)
                {
                    if (search.Key >= processing.Key)
                    {
                        continue;
                    }

                    foreach (var s in search.Value)
                    {
                        processing.Value.Add(s);
                    }
                }
            }
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
            var legacyUserRights = this.GetLegacyUserRights(user, ServiceLocator.Current.GetInstance<IIrcClient>());

            return this.flagMapping[legacyUserRights].Contains(flag);
        }

        public IEnumerable<string> GetFlagsForUser(IUser user, string locality)
        {
            var legacyUserRights = this.GetLegacyUserRights(user, ServiceLocator.Current.GetInstance<IIrcClient>());

            return this.flagMapping[legacyUserRights];
        }
    }
}