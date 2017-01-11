using Castle.Core.Logging;
using Helpmebot.Model;
using Helpmebot.Repositories.Interfaces;
using NHibernate;

namespace Helpmebot.Repositories
{
    public class BlockMonitorRepository : RepositoryBase<BlockMonitor>, IBlockMonitorRepository
    {
        public BlockMonitorRepository(ISession session, ILogger logger) : base(session, logger)
        {
        }
    }
}