namespace Helpmebot.Commands.Commands.ACL
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Standard)]
    // ReSharper disable once StringLiteralTypo
    [CommandInvocation("flaggroup")]
    // ReSharper disable once UnusedMember.Global
    public class FlagGroupCommand : CommandBase
    {
        private readonly IAccessControlManagementService aclManagementService;
        private readonly ISession session;
        private readonly IResponder responder;

        public FlagGroupCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IAccessControlManagementService aclManagementService,
            ISession session,
            IResponder responder) : base(
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
            this.responder = responder;
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
                yield return this.responder.Respond(
                    "commands.command.flaggroup.list",
                    this.CommandSource,
                    new object[] { flagGroup.Name, flagGroup.Flags }).First();
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
                return this.responder.Respond("commands.command.flaggroup.created", this.CommandSource);
                
            }
            catch (AclException ex)
            {
                return this.responder.Respond("commands.command.flaggroup.error", this.CommandSource, ex.Message);
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
                return this.responder.Respond("commands.command.flaggroup.modified", this.CommandSource);
            }
            catch (AclException ex)
            {
                return this.responder.Respond("commands.command.flaggroup.error", this.CommandSource, ex.Message);
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
        protected IEnumerable<CommandResponse> SetFlagsMode()
        {
            var tx = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                this.aclManagementService.SetFlagGroupFlags(this.Arguments[0], this.Arguments[1], this.session);
                tx.Commit();
                return this.responder.Respond("commands.command.flaggroup.updated", this.CommandSource);
            }
            catch (AclException ex)
            {
                return this.responder.Respond("commands.command.flaggroup.error", this.CommandSource, ex.Message);
            }
            finally
            {
                if (!tx.WasCommitted)
                {
                    tx.Rollback();
                }
            }
        }
        
        [SubcommandInvocation("mode")]
        [CommandFlag(Flags.AccessControl)]
        [RequiredArguments(2)]
        [Help("<name> granting|revoking", "Sets the mode the flag group operates in - either granting flags from a user, or revoking flags from a user.")]
        // ReSharper disable once UnusedMember.Global
        protected IEnumerable<CommandResponse> SetFlagGroupMode()
        {
            bool granting;
            switch (this.Arguments[1])
            {
                case "granting":
                case "grant":
                case "+":
                    granting = true;
                    break;
                case "revoking":
                case "revoke":
                case "-":
                    granting = false;
                    break;
                default:
                    return this.responder.Respond("commands.command.flaggroup.invalidmode", this.CommandSource);
            }
            
            var tx = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                this.aclManagementService.SetFlagGroupMode(this.Arguments[0], granting, this.session);
                tx.Commit();
                return this.responder.Respond("commands.command.flaggroup.updated", this.CommandSource);
            }
            catch (AclException ex)
            {
                return this.responder.Respond("commands.command.flaggroup.error", this.CommandSource, ex.Message);
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
                return this.responder.Respond("commands.command.flaggroup.deleted", this.CommandSource);
            }
            catch (AclException ex)
            {
                return this.responder.Respond("commands.command.flaggroup.error", this.CommandSource, ex.Message);
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