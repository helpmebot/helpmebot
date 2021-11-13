namespace Helpmebot.WebApi.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.WebApi.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;
    
    [CommandFlag(Flags.Standard)]
    [CommandInvocation("weblogin")]
    public class WebLoginCommand : CommandBase
    {
        private readonly ILoginTokenService loginTokenService;

        public WebLoginCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ILoginTokenService loginTokenService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.loginTokenService = loginTokenService;
        }

        [RequiredArguments(1)]
        [Help("<token>", "Completes a login to the web interface.")]    
        protected override IEnumerable<CommandResponse> Execute()
        {
            if (string.IsNullOrWhiteSpace(this.User.Account))
            {
                throw new CommandAccessDeniedException();
            }
            
            var approved = this.loginTokenService.ApproveLoginToken(this.Arguments.First(), this.User);

            if (approved)
            {
                yield return new CommandResponse { Message = "Login approved, please return to the web UI and click the 'Continue' button." };
            }
            else
            {
                yield return new CommandResponse { Message = "Login failed." };
            }
        }
    }
}