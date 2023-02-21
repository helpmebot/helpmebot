namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;
    using Helpmebot.Model;

    public class CrossChannelMap : ClassMap<CrossChannel>
    {
        public CrossChannelMap()
        {
            this.Table("crosschannel");
            this.Id(x => x.Id, "id");
            this.Map(x => x.FrontendChannel, "frontend_name");
            this.Map(x => x.BackendChannel, "backend_name");
            this.Map(x => x.NotifyEnabled, "notify_enabled");
            this.Map(x => x.NotifyKeyword, "notify_keyword");
            this.Map(x => x.NotifyMessage, "notify_message");
            this.Map(x => x.ForwardEnabled, "forward_enabled");
        }
    }
}