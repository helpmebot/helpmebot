using FluentNHibernate.Mapping;
using Helpmebot.Model;

namespace Helpmebot.Persistence.Mappings
{
    public class WelcomeLogMap : ClassMap<WelcomeLog>
    {
        public WelcomeLogMap()
        {
            this.Table("welcomelog");
            this.Id(x => x.Id, "id");
            this.Map(x => x.Usermask, "usermask");
            this.Map(x => x.Channel, "channel");
            this.Map(x => x.WelcomeTimestamp, "welcome_timestamp");
        }
    }
}