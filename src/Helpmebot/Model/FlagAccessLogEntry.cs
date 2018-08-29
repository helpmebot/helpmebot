namespace Helpmebot.Model
{
    using System;
    using Helpmebot.Persistence;

    public class FlagAccessLogEntry : EntityBase
    {
        public virtual DateTime Timestamp { get; set; }
        public virtual string Class { get; set; }
        public virtual string Invocation { get; set; }
        public virtual string Nickname { get; set; }
        public virtual string Username { get; set; }
        public virtual string Hostname { get; set; }
        public virtual string Account { get; set; }
        public virtual string Context { get; set; }
        public virtual string AvailableFlags { get; set; }
        public virtual string RequiredMainCommand { get; set; }
        public virtual string RequiredSubCommand { get; set; }
        public virtual string Result { get; set; }
    }
}