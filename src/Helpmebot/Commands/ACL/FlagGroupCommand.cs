namespace Helpmebot.Commands.ACL
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.BotInfo)]
    [CommandInvocation("flaggroup")]
    public class FlagGroupCommand : CommandBase
    {
        private readonly IAccessControlService aclService;
        private readonly ISession session;

        public FlagGroupCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IAccessControlService aclService,
            ISession session) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.aclService = aclService;
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
            var result = this.aclService.CreateFlagGroup(this.Arguments[0], this.Arguments[1], this.session);

            if (result)
            {
                yield return new CommandResponse {Message = "Done"};
            }
            else
            {
                yield return new CommandResponse {Message = "Failed to create a new flag group - does it already exist?"};
            }
        }

        [SubcommandInvocation("modify")]
        [CommandFlag(Flags.AccessControl)]
        [RequiredArguments(2)]
        [Help("<name> <flags>", "Modifies the flags on an existing flag group")]
        // ReSharper disable once UnusedMember.Global
        protected IEnumerable<CommandResponse> ModifyMode()
        {
            var result = this.aclService.ModifyFlagGroup(this.Arguments[0], this.Arguments[1], this.session);

            if (result)
            {
                yield return new CommandResponse {Message = "Done"};
            }
            else
            {
                yield return new CommandResponse {Message = "Failed to modify the flag group - does this group exist?"};
            }
        }

        [SubcommandInvocation("set")]
        [CommandFlag(Flags.AccessControl)]
        [RequiredArguments(2)]
        [Help("<name> <flags>", "Sets the flags on an existing flag group")]
        // ReSharper disable once UnusedMember.Global
        protected IEnumerable<CommandResponse> SetMode()
        {
            var result = this.aclService.SetFlagGroup(this.Arguments[0], this.Arguments[1], this.session);

            if (result)
            {
                yield return new CommandResponse {Message = "Done"};
            }
            else
            {
                yield return new CommandResponse {Message = "Failed to modify the flag group - does this group exist?"};
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
            var result = this.aclService.DeleteFlagGroup(this.Arguments[0], this.session);

            if (result)
            {
                yield return new CommandResponse {Message = "Done"};
            }
            else
            {
                yield return new CommandResponse {Message = "Failed to delete the flag group - does this group exist?"};
            }

        }
    }
}