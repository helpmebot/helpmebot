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
            this.Map(x => x.Mask, "mask");
            this.Map(x => x.Account, "account");
            this.Map(x => x.LastModified, "user_lastmodified");

            this.HasManyToMany(x => x.AppliedFlagGroups)
                .Cascade.All()
                .Table("flaggroup_user")
                .ParentKeyColumn("user")
                .ChildKeyColumn("flaggroup");
        }
    }
}