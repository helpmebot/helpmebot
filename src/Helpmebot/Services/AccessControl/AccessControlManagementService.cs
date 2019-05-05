namespace Helpmebot.Services.AccessControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Exceptions;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class AccessControlManagementService : IAccessControlManagementService
    {
        private readonly ILogger logger;

        public AccessControlManagementService(ILogger logger)
        {
            this.logger = logger;
        }

        #region Flag groups
        public void CreateFlagGroup(string name, string flags, ISession session)
        {
            this.logger.DebugFormat("Creating flag group {0} with flags {1}", name, flags);

            var rowcount = session.CreateCriteria<FlagGroup>()
                .Add(Restrictions.Eq("Name", name))
                .SetProjection(Projections.RowCount())
                .UniqueResult<int>();

            if (rowcount != 0)
            {
                throw new AclException("This flag group already exists.");
            }

            var newGroup = new FlagGroup
            {
                Name = name,
                Flags = flags,
                Mode = "+",
                LastModified = DateTime.Now
            };

            try
            {
                session.Save(newGroup);
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Error creating new flag group {0}", name);
                throw new AclException("Encountered unknown error while creating flag group.");
            }
        }

        public void ModifyFlagGroup(string name, string flagChanges, ISession session)
        {
            var item = session.CreateCriteria<FlagGroup>()
                .Add(Restrictions.Eq("Name", name))
                .UniqueResult<FlagGroup>();

            if (item == null)
            {
                throw new AclException("This flag group does not exist.");
            }

            // TODO: prevent flag changes where user does not hold that flag. This applies for this entire class.
            item.Flags = string.Join(
                "",
                ApplyFlagChangesToFlags(
                    ref flagChanges,
                    item.Flags.ToCharArray().Select(x => new string(new[] {x})),
                    new List<string>()));

            if (flagChanges == string.Empty)
            {
                throw new AclException("No valid flag changes were found to be processed");
            }

            this.logger.DebugFormat("Updating flag group {0} with flag changes {1} to {2}", name, flagChanges, item.Flags);

            try
            {
                session.Update(item);
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Error updating flag group {0}", name);
                throw new AclException("Encountered unknown error while updating flag group.");
            }
        }

        public void SetFlagGroup(string name, string flags, ISession session)
        {

            var item = session.CreateCriteria<FlagGroup>()
                .Add(Restrictions.Eq("Name", name))
                .UniqueResult<FlagGroup>();

            if (item == null)
            {
                throw new AclException("This flag group does not exist.");
            }

            item.Flags = flags;

            this.logger.DebugFormat("Updating flag group {0} with flags {1}", name, flags);

            try
            {
                session.Update(item);
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Error updating flag group {0}", name);
                throw new AclException("Encountered unknown error while updating flag group.");
            }
        }

        public void DeleteFlagGroup(string name, ISession session)
        {
            this.logger.DebugFormat("Deleting flag group {0}", name);

            var item = session.CreateCriteria<FlagGroup>()
                .Add(Restrictions.Eq("Name", name))
                .UniqueResult<FlagGroup>();

            if (item == null)
            {
                throw new AclException("This flag group does not exist.");
            }

            try
            {
                session.Delete(item);
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Error deleting flag group {0}", name);
                throw new AclException("Encountered unknown error while deleting flag group.");
            }
        }
        #endregion

        #region Global grants
        public void GrantFlagGroupGlobally(IUser user, FlagGroup group)
        {
            throw new NotImplementedException();
        }

        public void RevokeFlagGroupGlobally(IUser user, FlagGroup group)
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Applies a set of flag changes to the provided set of flags
        /// </summary>
        /// <param name="changes">The changes to make</param>
        /// <param name="original">The original set of flags</param>
        /// <param name="preventedChanges">A list of flags for which changes are disallowed</param>
        /// <returns></returns>
        public static IEnumerable<string> ApplyFlagChangesToFlags(ref string changes, IEnumerable<string> original, IList<string> preventedChanges)
        {
            var result = new HashSet<string>(original);
            var addMode = true;

            var validFlags = Flag.GetValidFlags();

            var newChanges = "";

            foreach (var changeChar in changes)
            {
                var c = changeChar.ToString();

                if (c == "-")
                {
                    addMode = false;
                    newChanges += c;
                    continue;
                }

                if (c == "+")
                {
                    addMode = true;
                    newChanges += c;
                    continue;
                }

                if (c == "*" && !addMode)
                {
                    foreach (var i in new List<string>(result))
                    {
                        if (preventedChanges.Contains(c))
                        {
                            continue;
                        }

                        result.Remove(i);
                        newChanges += i;
                    }

                    continue;
                }

                if (addMode)
                {
                    if (!validFlags.Contains(c))
                    {
                        continue;
                    }

                    if (preventedChanges.Contains(c))
                    {
                        continue;
                    }

                    if (result.Contains(c))
                    {
                        // no-op
                        continue;
                    }

                    newChanges += c;
                    result.Add(c);
                }
                else
                {
                    if (preventedChanges.Contains(c))
                    {
                        continue;
                    }

                    if (!result.Contains(c))
                    {
                        // no-op
                        continue;
                    }

                    newChanges += c;
                    result.Remove(c);
                }
            }

            newChanges = newChanges.TrimEnd('+', '-');

            changes = newChanges;
            return result;

        }
    }
}