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
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;

    public class AccessControlManagementService : IAccessControlManagementService
    {
        private readonly ILogger logger;
        private readonly IFlagService authService;

        public AccessControlManagementService(ILogger logger, IFlagService authService)
        {
            this.logger = logger;
            this.authService = authService;
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
            
            this.CleanUpUserEntries(session);
        }
        #endregion

        #region Global grants
        public void GrantFlagGroupGlobally(User user, FlagGroup group, ISession session)
        {
            this.logger.DebugFormat("Granting group {0} to {1} globally", group, user.Mask);

            var existing = session.CreateCriteria<FlagGroupUser>()
                .Add(Restrictions.Eq("User", user))
                .Add(Restrictions.Eq("FlagGroup", group))
                .UniqueResult<FlagGroupUser>();

            if (existing == null)
            {
                var fgu = new FlagGroupUser {User = user, FlagGroup = group, LastModified = DateTime.UtcNow};
                session.Save(fgu);
                ((AccessControlAuthorisationService) this.authService).Refresh(user);
            }
        }

        public void RevokeFlagGroupGlobally(User user, FlagGroup group, ISession session)
        {
            this.logger.DebugFormat("Revoking group {0} from {1} globally", group, user.Mask);

            var existing = session.CreateCriteria<FlagGroupUser>()
                .Add(Restrictions.Eq("User", user))
                .Add(Restrictions.Eq("FlagGroup", group))
                .UniqueResult<FlagGroupUser>();

            if (existing != null)
            {
                session.Delete(existing);
                ((AccessControlAuthorisationService) this.authService).Refresh(user);
            }

            this.CleanUpUserEntries(session);
        }
        
        #endregion

        /// <summary>
        /// Either creates or retrieves an existing entry for a specified user mask
        /// </summary>
        public User GetUserObject(string ircMask, ISession session)
        {
            string mask = null;
            string account = null;
            
            if (ircMask.Contains("!") || ircMask.Contains("@")|| ircMask.Contains("*"))
            {
                mask = ircMask;
            }
            else
            {
                account = ircMask;
            }

            var user = session.CreateCriteria<User>()
                .Add(Restrictions.Eq("Account", account))
                .Add(Restrictions.Eq("Mask", mask))
                .UniqueResult<User>();

            if (user != null)
            {
                return user;
            }

            user = new User
            {
                Mask = mask,
                Account = account,
                LastModified = DateTime.UtcNow
            };

            session.Save(user);
            return user;
        }
        
        // ReSharper disable once UnusedParameter.Local
        private void CleanUpUserEntries(ISession session)
        {
            this.logger.Debug("Cleaned up no user unused user entries. This function is not yet implemented.");
            // placeholder to clean up any user entries with no grants
        }

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