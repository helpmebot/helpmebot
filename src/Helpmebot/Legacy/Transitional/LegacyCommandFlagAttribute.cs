namespace Helpmebot.Legacy.Transitional
{
    using System;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;

    [AttributeUsage(AttributeTargets.Class)]
    public class LegacyCommandFlagAttribute : Attribute
    {
        public LegacyCommandFlagAttribute(LegacyUserRights flag)
        {
            switch (flag)
            {
                case LegacyUserRights.Ignored:
                    throw new InvalidOperationException("Cannot assign ignored flag");
                case LegacyUserRights.Semiignored:
                    this.Flag = Flags.LegacySemiignored;
                    break;
                case LegacyUserRights.Normal:
                    this.Flag = Flags.Standard;
                    break;
                case LegacyUserRights.Advanced:
                    this.Flag = Flags.LegacyAdvanced;
                    break;
                case LegacyUserRights.Superuser:
                    this.Flag = Flags.LegacySuperuser;
                    break;
                case LegacyUserRights.Developer:
                    this.Flag = "O";
                    break;
            }
        }

        public string Flag { get; private set; }

        public bool GlobalOnly
        {
            get { return true; }
        }
    }
}