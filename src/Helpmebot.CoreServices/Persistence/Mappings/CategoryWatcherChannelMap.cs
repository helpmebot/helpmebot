namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;
    using Helpmebot.Model;

    public class CategoryWatcherChannelMap : ClassMap<CategoryWatcherChannel>
    {
        public CategoryWatcherChannelMap()
        {
            this.Table("catwatcher_channel");

            this.Id(x => x.Id, "id");
            this.References(x => x.Channel, "channel");
            this.References(x => x.Watcher, "watcher");
            this.Map(x => x.SleepTime, "sleeptime");
            this.Map(x => x.ShowWaitTime, "showwait");
            this.Map(x => x.MinWaitTime, "minwait");
            this.Map(x => x.ShowLink, "showlink");
            this.Map(x => x.AlertForAdditions, "alertadditions");
            this.Map(x => x.AlertForRemovals, "alertremovals");
        }
    }
}