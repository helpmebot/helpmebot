namespace Helpmebot.Model
{
    using System;
    using Helpmebot.Persistence;

    public class User : EntityBase
    {
        public virtual string Nickname { get; set; }

        public virtual string Username { get; set; }

        public virtual string Hostname { get; set; }

        [Obsolete]
        public virtual string AccessLevel { get; set; }

        public virtual DateTime LastModified { get; set; }
    }
}