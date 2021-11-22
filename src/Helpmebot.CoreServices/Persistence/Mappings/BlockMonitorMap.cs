namespace Helpmebot.Persistence.Mappings
{
    using FluentNHibernate.Mapping;
    using Helpmebot.Model;

    public class BlockMonitorMap: ClassMap<BlockMonitor>
    {
        public BlockMonitorMap()
        {
            this.Table("blockmonitor");
            this.Id(x => x.Id);
            this.Map(x => x.MonitorChannel, "monitorchannel");
            this.Map(x => x.ReportChannel, "reportchannel");
        }
    }
}