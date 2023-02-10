namespace Helpmebot.Commands.Commands.WikiInformation
{
    using System;
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using CoreServices.Attributes;
    using Helpmebot.Commands.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Exceptions;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("registration")]
    [CommandInvocation("reg")]
    [CommandInvocation("age")]
    [CommandFlag(Flags.Info)]
    [HelpSummary("Returns the registration date and account age of the specified user")]
    public class RegistrationCommand : CommandBase
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;

        public RegistrationCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IMediaWikiApiHelper apiHelper,
            IResponder responder,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.apiHelper = apiHelper;
            this.responder = responder;
            this.channelManagementService = channelManagementService;
        }

        [Help("<username>")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiApi = this.apiHelper.GetApi(this.channelManagementService.GetBaseWiki(this.CommandSource));

            var username = string.Join(" ", this.Arguments);
            if (this.Arguments.Count == 0)
            {
                username = this.User.Nickname;
            }

            DateTime? registrationDate;
            try
            {
                registrationDate = mediaWikiApi.GetRegistrationDate(username);
            }
            catch (MediawikiApiException e)
            {
                this.Logger.WarnFormat(e, "Encountered error retrieving registration date from API for {0}", username);
                return this.responder.Respond("common.mw-api-error", this.CommandSource);
            }
            finally
            {
                this.apiHelper.Release(mediaWikiApi);
            }

            if (!registrationDate.HasValue)
            {
                return this.responder.Respond("commands.command.registration.none", this.CommandSource, username);
            }

            int years;
            TimeSpan age;
            registrationDate.Value.CalculateDuration(out years, out age);

            return this.responder.Respond(
                "commands.command.registration",
                this.CommandSource,
                new object[]
                {
                    username, registrationDate.Value, years, age
                });
        }

        
    }
}