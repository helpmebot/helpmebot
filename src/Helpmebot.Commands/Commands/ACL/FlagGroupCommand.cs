namespace Helpmebot.Commands.Commands.ACL
{
    using System.Collections.Generic;
    using System.Data;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.BotInfo)]
    // ReSharper disable once StringLiteralTypo
    [CommandInvocation("flaggroup")]
    // ReSharper disable once UnusedMember.Global
    public class FlagGroupCommand : CommandBase
    {
        private readonly IAccessControlManagementService aclManagementService;
        private readonly ISession session;

        public FlagGroupCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IAccessControlManagementService aclManagementService,
            ISession session) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.aclManagementService = aclManagementService;
            this.session = session;
        }

        protected override IEnumerable<CommandResponse> Execute()
        {
            return this.ListMode();
        }

        [SubcommandInvocation("list")]
        [Help("", "Lists the currently configured flag groups")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected IEnumerable<CommandResponse> ListMode()
        {
            var flagGroups = this.session.CreateCriteria<FlagGroup>().List<FlagGroup>();
            foreach (var flagGroup in flagGroups)
            {
                yield return new CommandResponse
                {
                    Message = string.Format("Group {0}: {1}", flagGroup.Name, flagGroup.Flags),
                    Type = CommandResponseType.Notice,
                    Destination = CommandResponseDestination.PrivateMessage
                };
            }
        }

        [SubcommandInvocation("add")]
        [SubcommandInvocation("create")]
        [CommandFlag(Flags.AccessControl)]
        [RequiredArguments(2)]
        [Help("<name> <flags>", "Adds a new flag group")]
        // ReSharper disable once UnusedMember.Global
        protected IEnumerable<CommandResponse> AddMode()
        {
            var tx = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                this.aclManagementService.CreateFlagGroup(this.Arguments[0], this.Arguments[1], this.session);
                tx.Commit();
                return new[] {new CommandResponse {Message = "Flag group created."}};
            }
            catch (AclException ex)
            {
                return new[]
                    {new CommandResponse {Message = string.Format("Error creating flag group: {0}", ex.Message)}};
            }
            finally
            {
                if (!tx.WasCommitted)
                {
                    tx.Rollback();
                }
            }
        }

        [SubcommandInvocation("modify")]
        [CommandFlag(Flags.AccessControl)]
        [RequiredArguments(2)]
        [Help("<name> <flag changes>", "Modifies the flags on an existing flag group")]
        // ReSharper disable once UnusedMember.Global
        protected IEnumerable<CommandResponse> ModifyMode()
        {
            var tx = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                this.aclManagementService.ModifyFlagGroup(this.Arguments[0], this.Arguments[1], this.session);
                tx.Commit();
                return new[] {new CommandResponse {Message = "Flag group modified."}};
            }
            catch (AclException ex)
            {
                return new[]
                    {new CommandResponse {Message = string.Format("Error modifying flag group: {0}", ex.Message)}};
            }
            finally
            {
                if (!tx.WasCommitted)
                {
                    tx.Rollback();
                }
            }
        }

        [SubcommandInvocation("set")]
        [CommandFlag(Flags.AccessControl)]
        [RequiredArguments(2)]
        [Help("<name> <flags>", "Sets the flags on an existing flag group")]
        // ReSharper disable once UnusedMember.Global
        protected IEnumerable<CommandResponse> SetMode()
        {
            var tx = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                this.aclManagementService.SetFlagGroup(this.Arguments[0], this.Arguments[1], this.session);
                tx.Commit();
                return new[] {new CommandResponse {Message = "Flag group updated."}};
            }
            catch (AclException ex)
            {
                return new[]
                    {new CommandResponse {Message = string.Format("Error updating flag group: {0}", ex.Message)}};
            }
            finally
            {
                if (!tx.WasCommitted)
                {
                    tx.Rollback();
                }
            }
        }

        [SubcommandInvocation("delete")]
        [SubcommandInvocation("remove")]
        [CommandFlag(Flags.AccessControl)]
        [RequiredArguments(1)]
        [Help("<name>", "Deletes an existing flag group")]
        // ReSharper disable once UnusedMember.Global
        protected IEnumerable<CommandResponse> DeleteMode()
        {
            var tx = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                this.aclManagementService.DeleteFlagGroup(this.Arguments[0], this.session);
                tx.Commit();
                return new[] {new CommandResponse {Message = "Flag group deleted."}};
            }
            catch (AclException ex)
            {
                return new[]
                    {new CommandResponse {Message = string.Format("Error deleting flag group: {0}", ex.Message)}};
            }
            finally
            {
                if (!tx.WasCommitted)
                {
                    tx.Rollback();
                }
            }
        }
    }
}