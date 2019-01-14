namespace Helpmebot.Commands.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.Model;
    using NDesk.Options;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("welcomer")]
    [CommandFlag(Flags.LocalConfiguration)]
    [CommandFlag(Flags.Configuration, true)]
    public class WelcomerConfigurationCommand : CommandBase
    {
        private readonly ISession databaseSession;

        public WelcomerConfigurationCommand(
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

        [SubcommandInvocation("list")]
        [Help("", "Lists all masks configured to be welcomed in this channel")]
        protected IEnumerable<CommandResponse> ListMode()
        {
            var welcomeForChannel =
                this.databaseSession.QueryOver<WelcomeUser>().Where(x => x.Channel == this.CommandSource).List();

            if (welcomeForChannel.Count == 0)
            {
                yield return new CommandResponse {Message = string.Format("Not welcoming in {0}", this.CommandSource)};
                yield break;
            }
            
            yield return new CommandResponse
            {
                Message = string.Format(
                    "Welcoming these masks to {0}: {1}",
                    this.CommandSource,
                    string.Join("  ", welcomeForChannel.Select(x => x.ToString())))
            };
        }

        [SubcommandInvocation("add")]
        [RequiredArguments(1)]
        [Help("[--ignore] <mask>", new[]{"Adds a mask to the welcome list for the current channel.", "Use the --ignore flag to make this an exception rule instead of a match rule."})]
        protected IEnumerable<CommandResponse> AddMode()
        {
            var exception = false;
            var opts = new OptionSet
            {
                {"ignore", x => exception = true}
            };
            var extra = opts.Parse(this.Arguments);

            this.databaseSession.BeginTransaction(IsolationLevel.RepeatableRead);
            try
            {
                var welcomeUser = new WelcomeUser
                {
                    Nick = ".*",
                    User = ".*",
                    Host = string.Join(" ", extra),
                    Channel = this.CommandSource,
                    Exception = exception
                };

                this.databaseSession.Save(welcomeUser);
                this.databaseSession.Transaction.Commit();
                
                return new[] {new CommandResponse {Message = "Done."}};
            }
            catch (Exception e)
            {
                this.Logger.Error("Error occurred during addition of welcome mask.", e);
                
                this.databaseSession.Transaction.Rollback();

                return new[] {new CommandResponse {Message = e.Message}};
            }
        }

        [SubcommandInvocation("del")]
        [SubcommandInvocation("delete")]
        [SubcommandInvocation("remove")]
        [RequiredArguments(1)]
        [Help("[--ignore] <mask>", new[]{"Adds a mask to the welcome list for the current channel.", "Use the --ignore flag to make this an exception rule instead of a match rule."})]
        protected IEnumerable<CommandResponse> DeleteMode()
        {
            var exception = false;
            var opts = new OptionSet
            {
                {"ignore", x => exception = true}
            };
            var extra = opts.Parse(this.Arguments);
            
            this.databaseSession.BeginTransaction(IsolationLevel.RepeatableRead);
            
            try
            {
                this.Logger.Debug("Getting list of welcomeusers ready for deletion!");

                var implode = string.Join(" ", extra);

                var welcomeUsers =
                    this.databaseSession.QueryOver<WelcomeUser>()
                        .Where(x => x.Exception == exception && x.Host == implode && x.Channel == this.CommandSource)
                        .List();

                this.Logger.Debug("Got list of WelcomeUsers, proceeding to Delete...");

                foreach (var welcomeUser in welcomeUsers)
                {
                    this.databaseSession.Delete(welcomeUser);
                }

                this.Logger.Debug("All done, cleaning up and sending message to IRC");

                this.databaseSession.Transaction.Commit();
                return new[] {new CommandResponse {Message = "Done."}};
            }
            catch (Exception e)
            {
                this.Logger.Error("Error occurred during addition of welcome mask.", e);

                this.databaseSession.Transaction.Rollback();
                
                return new[] {new CommandResponse {Message = e.Message}};
            }
        }
    }
}