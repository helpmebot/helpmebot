namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;
    using Helpmebot.Model;
    
    public class CategoryItemMap : ClassMap<CategoryItem>
    {
        public CategoryItemMap()
        {
            this.Table("categoryitems");
            this.Id(x => x.Id, "id");
            this.Map(x => x.Title, "title");
            this.Map(x => x.InsertTime, "inserttime");
            this.References(x => x.Watcher, "watcher");
        }
    }
}