namespace Helpmebot.Commands.Commands.FunCommands
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public abstract class TargetedFunCommandBase : FunCommandBase
    {
        private string commandTarget;
        
        protected TargetedFunCommandBase(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
            IMessageService messageService,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client,
            databaseSession,
            messageService,
            responder)
        {
        }
        
        protected virtual string CommandTarget
        {
            get
            {
                if (this.commandTarget == null)
                {
                    this.commandTarget = this.GetCommandTarget();
                }

                return this.commandTarget;
            }
        }
        
        private string GetCommandTarget()
        {
            if (this.Arguments.Any())
            {
                return string.Join(" ", this.Arguments);
            }

            if (this.RedirectionTarget.Any())
            {
                return this.RedirectionTarget.First();
            }

            return this.User.Nickname;
        }

        protected IEnumerable<CommandResponse> GetResponse(string message)
        {
            string[] messageparams = { this.CommandTarget };
            string response = this.MessageService.RetrieveMessage(message, this.CommandSource, messageparams);

            if (response == null)
            {
                this.Logger.Error($"Message not found for key {message}");
                yield break;
            }
            
            yield return new CommandResponse
            {
                Message = response,
                IgnoreRedirection = true
            };
        }
    }
}