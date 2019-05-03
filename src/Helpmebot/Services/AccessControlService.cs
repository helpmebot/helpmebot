namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class AccessControlService : IAccessControlService
    {
        private readonly ISession globalSession;
        private readonly ILogger logger;

        public AccessControlService(ISession globalSession, ILogger logger)
        {
            this.globalSession = globalSession;
            this.logger = logger;
        }

        #region ACL checks
        public bool UserHasFlag(IUser user, string flag, string locality)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetFlagsForUser(IUser user, string locality)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Flag groups
        public bool CreateFlagGroup(string name, string flags, ISession session)
        {
            this.logger.DebugFormat("Creating flag group {0} with flags {1}", name, flags);

            var rowcount = session.CreateCriteria<FlagGroup>()
                .Add(Restrictions.Eq("Name", name))
                .SetProjection(Projections.RowCount())
                .UniqueResult<int>();

            if (rowcount != 0)
            {
                return false;
            }

            var newGroup = new FlagGroup
            {
                Name = name,
                Flags = flags,
                LastModified = DateTime.Now
            };

            try
            {
                session.Save(newGroup);
                session.Flush();
                return true;
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Error creating new flaggroup {0}", name);
                return false;
            }
        }

        public bool ModifyFlagGroup(string name, string flagchanges, ISession session)
        {
            var item = session.CreateCriteria<FlagGroup>()
                .Add(Restrictions.Eq("Name", name))
                .UniqueResult<FlagGroup>();

            if (item == null)
            {
                return false;
            }

            item.Flags = flagchanges;

            this.logger.DebugFormat("Updating flag group {0} with flag changes {1} to {2}", name, flagchanges, item.Flags);

            try
            {
                session.Update(item);
                session.Flush();
                return true;
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Error updating flaggroup {0}", name);
                return false;
            }
        }

        public bool SetFlagGroup(string name, string flags, ISession session)
        {

            var item = session.CreateCriteria<FlagGroup>()
                .Add(Restrictions.Eq("Name", name))
                .UniqueResult<FlagGroup>();

            if (item == null)
            {
                return false;
            }

            item.Flags = flags;

            this.logger.DebugFormat("Updating flag group {0} with flags {1}", name, flags);

            try
            {
                session.Update(item);
                session.Flush();
                return true;
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Error updating flaggroup {0}", name);
                return false;
            }
        }

        public bool DeleteFlagGroup(string name, ISession session)
        {
            this.logger.DebugFormat("Deleting flag group {0}", name);

            var item = session.CreateCriteria<FlagGroup>()
                .Add(Restrictions.Eq("Name", name))
                .UniqueResult<FlagGroup>();

            if (item == null)
            {
                return false;
            }

            try
            {
                session.Delete(item);
                session.Flush();
                return true;
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Error deleting flaggroup {0}", name);
                return false;
            }
        }
        #endregion

        #region Global grants
        public bool GrantFlagGroupGlobally(IUser user, FlagGroup @group)
        {
            throw new System.NotImplementedException();
        }

        public bool RevokeFlagGroupGlobally(IUser user, FlagGroup @group)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}