namespace Helpmebot.Services.AccessControl
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class AccessControlAuthorisationService : IFlagService
    {
        private readonly ISession session;
        private readonly ILogger logger;

        public AccessControlAuthorisationService(ISession session, ILogger logger)
        {
            this.session = session;
            this.logger = logger;
        }

        public bool UserHasFlag(IUser user, string flag, string locality)
        {
            return this.GetFlagsForUser(user, locality).Contains(flag);
        }

        public IEnumerable<string> GetFlagsForUser(IUser user, string locality)
        {
            var ircUser = user as IrcUser;
            if (ircUser == null)
            {
                return new string[0];
            }

            var matchingUsers = this.session.QueryOver<User>()
                .List<User>()
                .Where(
                    x => new IrcUserMask(x.Mask, ircUser.Client).Matches(user).GetValueOrDefault()
                         || (x.Account == user.Account && x.Account != null && user.Account != null))
                .ToList();

            var flagGroups = new List<FlagGroup>();
            foreach (var matchingUser in matchingUsers)
            {
                foreach (var group in matchingUser.AppliedFlagGroups)
                {
                    flagGroups.Add(group);
                }
            }

            this.logger.DebugFormat("Fetched {1} flag groups for {0}", user, flagGroups.Count);
                foreach (var group in flagGroups)
                {
                    if (group == null)
                    {
                        this.logger.DebugFormat("       -> null group");
                    }
                    else
                    {
                        this.logger.DebugFormat("       -> {0}", group.Name);
                    }
                }

            flagGroups = flagGroups.Distinct().Where(x => x != null).ToList();

            var changes = flagGroups.Where(x => x.Mode == "+").Aggregate("+", (s, group) => s + group.Flags);
            changes = flagGroups.Where(x => x.Mode == "-").Aggregate(changes + "-", (s, group) => s + group.Flags);

            this.logger.DebugFormat(
                "Found flag groups {0} (consisting of {2}) apply to {1}",
                flagGroups.Aggregate("", (s, g) => s + "," + g.Name).TrimStart(','),
                user,
                changes);

            var resultantSet =
                AccessControlManagementService.ApplyFlagChangesToFlags(
                    ref changes,
                    new List<string>(),
                    new List<string>());

            return resultantSet;
        }
    }
}
