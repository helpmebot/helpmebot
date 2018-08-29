namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;
    using Helpmebot.Model;

    public class FlagAccessLogEntryMap : ClassMap<FlagAccessLogEntry>
    {
        public FlagAccessLogEntryMap()
        {
            this.Table("flagaccesslog");
            this.Id(x => x.Id);
            this.Map(x => x.Timestamp);
            this.Map(x => x.Class);
            this.Map(x => x.Invocation);
            this.Map(x => x.Nickname);
            this.Map(x => x.Username);
            this.Map(x => x.Hostname);
            this.Map(x => x.Account);
            this.Map(x => x.Context);
            this.Map(x => x.AvailableFlags);
            this.Map(x => x.RequiredMainCommand, "requiredmain");
            this.Map(x => x.RequiredSubCommand, "requiredsub");
            this.Map(x => x.Result);
        }
    }
}