namespace Helpmebot.Legacy.Transitional
{
    using System.Collections.Generic;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class LegacyFlagService : IFlagService
    {
        private readonly ILegacyAccessService legacyAccessService;

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
                        Flags.LegacySuperuser, Flags.AccessControl, Flags.BotManagement, Flags.Brain,
                        Flags.Configuration, Flags.Uncurl
                    }
                },
                {
                    LegacyUserRights.Advanced,
                    new HashSet<string>
                        {Flags.LegacyAdvanced, Flags.Acc, Flags.Fun, Flags.LocalConfiguration, Flags.Protected}
                },
                {
                    LegacyUserRights.Normal,
                    new HashSet<string> {Flag.Standard, Flags.BotInfo, Flags.Info}
                },
                {
                    LegacyUserRights.Semiignored,
                    new HashSet<string> {Flags.LegacySemiignored}
                },
            };

        public LegacyFlagService(ILegacyAccessService legacyAccessService)
        {
            this.legacyAccessService = legacyAccessService;

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
        
        public bool UserHasFlag(IUser user, string flag, string locality)
        {
            var legacyUserRights = this.legacyAccessService.GetLegacyUserRights(user);

            return this.flagMapping[legacyUserRights].Contains(flag);
        }

        public IEnumerable<string> GetFlagsForUser(IUser user, string locality)
        {
            var legacyUserRights = this.legacyAccessService.GetLegacyUserRights(user);

            return this.flagMapping[legacyUserRights];
        }
    }
}