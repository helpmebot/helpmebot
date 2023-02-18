namespace Helpmebot.ChannelServices.Commands.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("welcomer")]
    [CommandFlag(Flags.LocalConfiguration)]
    [CommandFlag(Flags.Configuration, true)]
    [HelpSummary("Manages the on-join welcome message")]
    public class WelcomerConfigurationCommand : CommandBase
    {
        private readonly ISession databaseSession;
        private readonly IResponder responder;
        private readonly IJoinMessageConfigurationService joinMessageConfigurationService;
        private readonly IChannelManagementService channelManagementService;

        public WelcomerConfigurationCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession databaseSession,
            IResponder responder,
            IJoinMessageConfigurationService joinMessageConfigurationService,
            IChannelManagementService channelManagementService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.databaseSession = databaseSession;
            this.responder = responder;
            this.joinMessageConfigurationService = joinMessageConfigurationService;
            this.channelManagementService = channelManagementService;
        }

        protected override IEnumerable<CommandResponse> OnPreRun(out bool abort)
        {
            var target = this.Parameters.GetParameter("target", this.CommandSource);
            if (target != this.CommandSource && !this.FlagService.UserHasFlag(this.User, Flags.Configuration, null))
            {
                throw new CommandAccessDeniedException();
            }

            return base.OnPreRun(out abort);
        }

        [SubcommandInvocation("list")]
        [Help("", "Lists all masks configured to be welcomed in this channel")]
        [CommandParameter("target=", "The target channel to apply this command to", "target", typeof(string))]
        protected IEnumerable<CommandResponse> ListMode()
        {
            var target = this.Parameters.GetParameter("target", this.CommandSource);

            var welcomeForChannel = this.joinMessageConfigurationService.GetUsers(target);

            if (welcomeForChannel.Count == 0)
            {
                return this.responder.Respond("channelservices.command.welcomer.not-welcoming", this.CommandSource, target);
            }

            var allEntries = new List<WelcomeUser>();
            allEntries.AddRange(welcomeForChannel);
            allEntries.AddRange(this.joinMessageConfigurationService.GetExceptions(target));

            var commandResponses = this.responder.Respond(
                    "channelservices.command.welcomer.list.start",
                    this.CommandSource,
                    new object[] { target })
                .ToList();

            var listItems = allEntries.Select(
                x => new CommandResponse
                {
                    Message = this.responder.GetMessagePart(
                        "channelservices.command.welcomer.list.item",
                        this.CommandSource,
                        x.ToString())
                });

            commandResponses.AddRange(listItems);

            return commandResponses;
        }

        [SubcommandInvocation("add")]
        [Help("", new[]{"Adds a mask to the welcome list for the current channel.", "All attributes default to `.*`; use the listed options to override specific attributes."})]
        [CommandParameter("target=", "The target channel to apply this command to", "target", typeof(string))]
        [CommandParameter("ignore", "Interpret this mask as an exception rule instead of a match rule", "exception", typeof(bool))]
        [CommandParameter("host=", "Specify the hostname part of the mask to use", "host", typeof(string))]
        [CommandParameter("user=", "Specify the username part of the mask to use", "user", typeof(string))]
        [CommandParameter("nick=", "Specify the hostname part of the mask to use", "nick", typeof(string))]
        [CommandParameter("account=", "Specify the NickServ account part of the mask to use", "account", typeof(string))]
        [CommandParameter("realname=", "Specify the GECOS/realname part of the mask to use", "realname", typeof(string))]
        protected IEnumerable<CommandResponse> AddMode()
        {
            var target = this.Parameters.GetParameter("target", this.CommandSource);
            var exception = this.Parameters.GetParameter("exception", false);

            var host = this.Parameters.GetParameter("host", ".*");
            var nick = this.Parameters.GetParameter("nick", ".*");
            var user = this.Parameters.GetParameter("user", ".*");
            var account = this.Parameters.GetParameter("account", ".*");
            var realname = this.Parameters.GetParameter("realname", ".*");

            if (!this.ValidateRegex(nick, user, host, account, realname, out var responses))
            {
                return responses;
            }

            try
            {
                var welcomeUser = new WelcomeUser
                {
                    Nick = nick,
                    User = user,
                    Host = host,
                    Account = account,
                    RealName = realname,
                    Channel = target,
                    Exception = exception
                };

                this.joinMessageConfigurationService.AddWelcomeEntry(welcomeUser);
                return this.responder.Respond("common.done", this.CommandSource);
            }
            catch (Exception e)
            {
                return new[] { new CommandResponse { Message = e.Message } };
            }
        }

        private bool ValidateRegex(
            string nick,
            string user,
            string host,
            string account,
            string realname,
            out IEnumerable<CommandResponse> o)
        {
            var errors = new List<CommandResponse>();
            var valid = true;

            try
            {
                Regex.Match("", nick);
            }
            catch (ArgumentException ex)
            {
                valid = false;
                errors.Add(new CommandResponse { Message = "Error validating nick field: " + ex.Message });
            }

            try
            {
                Regex.Match("", user);
            }
            catch (ArgumentException ex)
            {
                valid = false;
                errors.Add(new CommandResponse { Message = "Error validating user field: " + ex.Message });
            }

            try
            {
                Regex.Match("", host);
            }
            catch (ArgumentException ex)
            {
                valid = false;
                errors.Add(new CommandResponse { Message = "Error validating host field: " + ex.Message });
            }

            try
            {
                Regex.Match("", account);
            }
            catch (ArgumentException ex)
            {
                valid = false;
                errors.Add(new CommandResponse { Message = "Error validating account field: " + ex.Message });
            }

            try
            {
                Regex.Match("", realname);
            }
            catch (ArgumentException ex)
            {
                valid = false;
                errors.Add(new CommandResponse { Message = "Error validating realname field: " + ex.Message });
            }

            o = errors;
            return valid;
        }

        [SubcommandInvocation("del")]
        [SubcommandInvocation("delete")]
        [SubcommandInvocation("remove")]
        [Help("", "Removes a mask from the welcome list for the current channel.")]
        [CommandParameter("target=", "The target channel to apply this command to", "target", typeof(string))]
        [CommandParameter("ignore", "Interpret this mask as an exception rule instead of a match rule", "exception", typeof(bool))]
        [CommandParameter("host=", "Specify the hostname part of the mask to use", "host", typeof(string))]
        [CommandParameter("user=", "Specify the username part of the mask to use", "user", typeof(string))]
        [CommandParameter("nick=", "Specify the hostname part of the mask to use", "nick", typeof(string))]
        [CommandParameter("account=", "Specify the NickServ account part of the mask to use", "account", typeof(string))]
        [CommandParameter("realname=", "Specify the GECOS/realname part of the mask to use", "realname", typeof(string))]
        protected IEnumerable<CommandResponse> DeleteMode()
        {
            var target = this.Parameters.GetParameter("target", this.CommandSource);
            var exception = this.Parameters.GetParameter("exception", false);

            var host = this.Parameters.GetParameter("host", ".*");
            var nick = this.Parameters.GetParameter("nick", ".*");
            var user = this.Parameters.GetParameter("user", ".*");
            var account = this.Parameters.GetParameter("account", ".*");
            var realname = this.Parameters.GetParameter("realname", ".*");

            if (!this.ValidateRegex(nick, user, host, account, realname, out var responses))
            {
                return responses;
            }

            try
            {
                Func<WelcomeUser, bool> searchPredicate = x =>
                    x.Exception == exception &&
                    x.Host == host &&
                    x.User == user &&
                    x.Nick == nick &&
                    x.Account == account &&
                    x.RealName == realname;

                var welcomeUsers = this.joinMessageConfigurationService.GetUsers(target).Where(searchPredicate);
                var exceptions = this.joinMessageConfigurationService.GetExceptions(target).Where(searchPredicate);

                this.Logger.Trace("Got list of WelcomeUsers, proceeding to Delete...");

                foreach (var welcomeUser in welcomeUsers.Union(exceptions))
                {
                    this.joinMessageConfigurationService.RemoveWelcomeEntry(welcomeUser);
                }

                this.Logger.Trace("All done, cleaning up and sending message to IRC");

                return this.responder.Respond("common.done", this.CommandSource);
            }
            catch (Exception e)
            {
                return new[] { new CommandResponse { Message = e.Message } };
            }
        }

        [RequiredArguments(1)]
        [SubcommandInvocation("mode")]
        [SubcommandInvocation("override")]
        [SubcommandInvocation("overridemode")]
        [Help(
            new[] { "none", "<mode>" },
            new[]
            {
                "Sets the welcomer override mode",
                "This enables a specific override rule for the welcomer allowing a different welcome message to be used for users matching pre-defined conditions"
            })]
        [CommandParameter("target=", "The target channel to apply this command to", "target", typeof(string))]
        protected IEnumerable<CommandResponse> WelcomerMode()
        {
            var target = this.Parameters.GetParameter("target", this.CommandSource);
            var requestedMode = this.Arguments[0];
            var flagName = (string)null;

            if (requestedMode != "none")
            {
                if (this.joinMessageConfigurationService.GetOverridesForChannel(target).Contains(requestedMode))
                {
                    flagName = requestedMode;
                }
                else
                {
                    return this.responder.Respond(
                        "channelservices.command.welcomer.override-not-found",
                        this.CommandSource,
                        requestedMode);
                }
            }

            this.channelManagementService.SetWelcomerFlag(target, flagName);
            return this.responder.Respond("common.done", this.CommandSource);
        }
    }
}