namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;
    using Helpmebot.Model;

    public class WelcomerOverrideMap : ClassMap<WelcomerOverride>
    {
        public WelcomerOverrideMap()
        {
            this.Table("welcomeroverride");
            this.Id(x => x.Id, "id");
            this.References(x => x.Channel, "channel");
            this.Map(x => x.ActiveFlag);
            this.Map(x => x.Geolocation);
            this.Map(x => x.BlockMessage);
            this.Map(x => x.Message);
            this.Map(x => x.ExemptNonMatching);
        }
    }
}