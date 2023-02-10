namespace Helpmebot.Commands.Commands.ACL
{
    using System.Collections.Generic;
    using System.Data;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.AccessControl;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.AccessControl)]
    [CommandInvocation("access")]
    [HelpSummary("Manages user access to the bot")]
    // ReSharper disable once UnusedMember.Global
    public class AccessCommand : CommandBase
    {
        private readonly IAccessControlManagementService accessControlManagementService;
        private readonly ISession session;
        private readonly IResponder responder;

        public AccessCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IAccessControlManagementService accessControlManagementService,
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
            this.accessControlManagementService = accessControlManagementService;
            this.session = session;
            this.responder = responder;
        }

        
        // ReSharper disable once UnusedMember.Global
        [Help("global <mask> <flaggroup>", "Grants the specified flag group to the user globally.")]
        [SubcommandInvocation("grant")]
        [RequiredArguments(3)]
        protected IEnumerable<CommandResponse> GrantMode()
        {
            var tx = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var user = this.accessControlManagementService.GetUserObject(this.Arguments[1], this.session);
                var flagGroupName = this.Arguments[2];

                var flagGroup = this.session.CreateCriteria<FlagGroup>()
                    .Add(Restrictions.Eq(nameof(FlagGroup.Name), flagGroupName))
                    .UniqueResult<FlagGroup>();

                if (flagGroup == null)
                {
                    return this.responder.Respond("commands.command.access.no-flag-group", this.CommandSource);
                }

                this.accessControlManagementService.GrantFlagGroupGlobally(user, flagGroup, this.session);
                tx.Commit();
                
                ((AccessControlAuthorisationService) this.FlagService).Refresh(user);
                
                return this.responder.Respond("common.done", this.CommandSource);
            }
            finally
            {
                if (!tx.WasCommitted)
                {
                    tx.Rollback();
                }
            }
        }
        
        // ReSharper disable once UnusedMember.Global
        [Help("global <mask> <flaggroup>", "Revokes the specified flag group from the user globally.")]
        [SubcommandInvocation("revoke")]
        [RequiredArguments(3)]
        protected IEnumerable<CommandResponse> RevokeMode()
        {
            var tx = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var user = this.accessControlManagementService.GetUserObject(this.Arguments[1], this.session);
                var flagGroupName = this.Arguments[2];

                var flagGroup = this.session.CreateCriteria<FlagGroup>()
                    .Add(Restrictions.Eq(nameof(FlagGroup.Name), flagGroupName))
                    .UniqueResult<FlagGroup>();

                if (flagGroup == null)
                {
                    return this.responder.Respond("commands.command.access.no-flag-group", this.CommandSource);
                }

                this.accessControlManagementService.RevokeFlagGroupGlobally(user, flagGroup, this.session);
                
                tx.Commit();
                
                return this.responder.Respond("common.done", this.CommandSource);
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