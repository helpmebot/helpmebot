using System;
using FluentNHibernate.Mapping;
using Helpmebot.Model;

namespace Helpmebot.Persistence.Mappings
{
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