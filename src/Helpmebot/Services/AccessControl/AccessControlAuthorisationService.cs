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
                         || (x.Account == user.Account && x.Account != null && user.Account != null));

            var flagGroups = matchingUsers.SelectMany(x => x.AppliedFlagGroups).ToList();

            this.logger.DebugFormat(
                "Found flag groups {0} apply to {1}",
                flagGroups.Distinct().Aggregate("", (s, g) => s + "," + g.Name).TrimStart(','),
                user);

            var changes = flagGroups.Where(x => x.Mode == "+").Aggregate("+", (s, group) => s + group.Flags);
            changes = flagGroups.Where(x => x.Mode == "-").Aggregate(changes + "-", (s, group) => s + group.Flags);

            var resultantSet =
                AccessControlManagementService.ApplyFlagChangesToFlags(
                    ref changes,
                    new List<string>(),
                    new List<string>());

            return resultantSet;
        }
    }
}