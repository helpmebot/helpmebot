namespace Helpmebot.Model
{
    using System;
    using Helpmebot.Persistence;

    public class User : EntityBase
    {
        [Obsolete]
        public virtual string Nickname { get; set; }
        [Obsolete]
        public virtual string Username { get; set; }
        [Obsolete]
        public virtual string Hostname { get; set; }

        public virtual string Mask { get; set; }

        public virtual string Account { get; set; }

        [Obsolete]
        public virtual string AccessLevel { get; set; }

        public virtual DateTime? LastModified { get; set; }
    }
}