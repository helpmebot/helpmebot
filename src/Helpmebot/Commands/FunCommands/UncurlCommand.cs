namespace Helpmebot.Commands.FunCommands
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Uncurl)]
    [CommandInvocation("uncurl")]
    public class UncurlCommand : CommandBase
    {
        private readonly ISession session;

        public UncurlCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession session) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.session = session;
        }

        [Help("", "Enables all fun commands in the current channel.")]        
        protected override IEnumerable<CommandResponse> Execute()
        {
            var channel = this.session.CreateCriteria<Channel>()
                .Add(Restrictions.Eq("Name", this.CommandSource))
                .UniqueResult<Channel>();
            
            if (channel == null)
            {
                yield return new CommandResponse
                {
                    Message = string.Format("Cannot find configuration for channel {0}", this.CommandSource),
                    IgnoreRedirection = true
                };
                yield break;
            }
            
            channel.HedgehogMode = false;
            this.session.SaveOrUpdate(channel);
            this.session.Flush();
            
            yield return new CommandResponse
            {
                Message = "Done"
            };
            
            yield return new CommandResponse
            {
                Message = string.Format("All fun commands are now enabled in {0}", this.CommandSource),
                Destination = CommandResponseDestination.PrivateMessage,
                IgnoreRedirection = true
            };
        }
    }
}