namespace Helpmebot.Commands.Commands.FunCommands
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
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
            IResponder responder,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client,
            responder,
            channelManagementService)
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
            object[] messageparams = { this.CommandTarget };

            return this.Responder.Respond(message, this.CommandSource, messageparams, ignoreRedirection: true);
        }
    }
}