namespace Helpmebot.Model
{
    using System;
    using System.Collections.Generic;
    using Helpmebot.Persistence;

    public class User : EntityBase
    {
        public virtual string Mask { get; set; }

        public virtual string Account { get; set; }

        public virtual DateTime? LastModified { get; set; }

        public virtual IList<FlagGroup> AppliedFlagGroups { get; set; }
    }
}