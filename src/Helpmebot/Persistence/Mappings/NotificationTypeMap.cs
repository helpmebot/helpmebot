namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;
    using Helpmebot.Model;

    public class NotificationTypeMap : ClassMap<NotificationType>
    {
        public NotificationTypeMap()
        {
            this.Table("notificationtype");
            this.Id(x => x.Id, "id");
            this.Map(x => x.Type, "type");
            this.References(x => x.Channel, "channel");
        }
    }
}