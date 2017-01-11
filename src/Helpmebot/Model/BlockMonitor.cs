using Helpmebot.Persistence;

namespace Helpmebot.Model
{
    public class BlockMonitor : EntityBase
    {
        public virtual string MonitorChannel { get; set; }
        public virtual string ReportChannel { get; set; }
    }
}