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
            this.Map(x => x.Channel, "channel_name");
            this.Map(x => x.Watcher, "watcher_keyword");
            this.Map(x => x.SleepTime, "sleeptime");
            this.Map(x => x.ShowWaitTime, "showwait");
            this.Map(x => x.MinWaitTime, "minwait");
            this.Map(x => x.ShowLink, "showlink");
            this.Map(x => x.AlertForAdditions, "alertadditions");
            this.Map(x => x.AlertForRemovals, "alertremovals");
            this.Map(x => x.Enabled, "enabled");
            this.Map(x => x.StatusMsg, "statusmsg");
            this.Map(x => x.Anchor, "anchor");
        }
    }
}