namespace Helpmebot.Commands.WikiInformation
{
    using System;
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using NHibernate;
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
    public class RegistrationCommand : CommandBase
    {
        private readonly ISession databaseSession;

        public RegistrationCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.databaseSession = databaseSession;
        }

        [Help("<username>", "Returns the registration date and account age of the specified user")]
        
        protected override IEnumerable<CommandResponse> Execute()
        {
            var mediaWikiSiteObject = this.databaseSession.GetMediaWikiSiteObject(this.CommandSource);

            var username = string.Join(" ", this.Arguments);
            if (this.Arguments.Count == 0)
            {
                username = this.User.Nickname;
            }

            DateTime? registrationDate;
            try
            {
                registrationDate = mediaWikiSiteObject.GetRegistrationDate(username);
            }
            catch (MediawikiApiException e)
            {
                this.Logger.WarnFormat(e, "Encountered error retrieving registration date from API for {0}", username);
                return new[] {new CommandResponse {Message = "Encountered error retrieving result from API"}};
            }

            if (!registrationDate.HasValue)
            {
                return new[] {new CommandResponse {Message = "No registration date found for the specified user."}};
            }

            var now = DateTime.Now;

            var backupVar = registrationDate.Value;
            var calcVar = registrationDate.Value;
            var years = -1;
            while (calcVar < now)
            {
                years++;
                backupVar = calcVar;
                calcVar = calcVar.AddYears(1);
            }

            var age = now - backupVar;
            
            var message = "[[User:{0}]] registered on {1:u} ({2}y {3:d\\d\\ hh\\:mm\\:ss} ago)";
            return new[]
            {
                new CommandResponse
                {
                    Message = string.Format(message, username, registrationDate.Value, years, age)
                }
            };
        }
    }
}