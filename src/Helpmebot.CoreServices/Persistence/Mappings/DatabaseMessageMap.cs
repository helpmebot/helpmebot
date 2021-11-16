namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;
    using Helpmebot.CoreServices.Model;
    
    public class DatabaseMessageMap : ClassMap<DatabaseMessage>
    {
        public DatabaseMessageMap()
        {
            this.Table("message");
            this.Id(k => k.Id).Column("id");
            this.Map(k => k.ContextType).Column("contexttype");
            this.Map(k => k.Context).Column("context");
            this.Map(k => k.MessageKey).Column("messagekey");
            this.Map(k => k.Value).Column("value");
            this.Map(k => k.Format).Column("format");
            this.Map(k => k.LastUpdated).Column("lastupdated");
        }
    }
}
