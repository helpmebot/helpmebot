namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;
    using Helpmebot.Model;

    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            this.Table("user");
            this.Id(x => x.Id, "user_id");
            this.Map(x => x.Nickname, "user_nickname");
            this.Map(x => x.Username, "user_username");
            this.Map(x => x.Hostname, "user_hostname");
            this.Map(x => x.AccessLevel, "user_accesslevel");
            this.Map(x => x.LastModified, "user_lastmodified");
        }
    }
}