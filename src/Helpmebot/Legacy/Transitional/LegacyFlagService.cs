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

        public LegacyFlagService(ILegacyAccessService legacyAccessService)
        {
            this.legacyAccessService = legacyAccessService;
        }
        
        public bool UserHasFlag(IUser user, string flag, string locality)
        {
            var legacyUserRights = this.legacyAccessService.GetLegacyUserRights(user);

            switch (flag)
            {
                case Flag.Owner:
                    return legacyUserRights == LegacyUserRights.Developer;
                case Flags.LegacySuperuser:
                    return legacyUserRights == LegacyUserRights.Superuser;
                case Flags.LegacyAdvanced:
                    return legacyUserRights == LegacyUserRights.Advanced;
                case Flag.Standard:
                    return legacyUserRights == LegacyUserRights.Normal;
                case Flags.LegacySemiignored:
                    return legacyUserRights == LegacyUserRights.Semiignored;
            }

            return false;
        }

        public IEnumerable<string> GetFlagsForUser(IUser user, string locality)
        {
            var flags = new List<string>();
            var legacyUserRights = this.legacyAccessService.GetLegacyUserRights(user);

            if (legacyUserRights >= LegacyUserRights.Semiignored)
            {
                flags.Add(Flags.LegacySemiignored);
            }
            
            if (legacyUserRights >= LegacyUserRights.Normal)
            {
                flags.Add(Flag.Standard);
            }
            
            if (legacyUserRights >= LegacyUserRights.Advanced)
            {
                flags.Add(Flags.LegacyAdvanced);
            }
            
            if (legacyUserRights >= LegacyUserRights.Superuser)
            {
                flags.Add(Flags.LegacySuperuser);
            }
            
            if (legacyUserRights >= LegacyUserRights.Developer)
            {
                flags.Add(Flag.Owner);
            }

            return flags;
        }
    }
}