namespace Helpmebot.Commands.Commands.BotManagement
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.Model;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flags.Configuration)]
    [CommandInvocation("interwiki")]
    [CommandInvocation("iw")]
    [HelpSummary("Manages the bot's list of interwiki prefixes for constructing links.")]
    public class InterwikiCommand : CommandBase
    {
        private readonly ISession session;

        public InterwikiCommand(
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
        
        [Help("<interwiki> <url>", "Adds or updates the specified interwiki entry")]
        [SubcommandInvocation("add")]
        [SubcommandInvocation("insert")]
        [SubcommandInvocation("edit")]
        [SubcommandInvocation("update")]
        [RequiredArguments(2)]
        protected IEnumerable<CommandResponse> Add()
        {
            var existing = this.session.QueryOver<InterwikiPrefix>()
                .Where(x => x.Prefix == this.Arguments[0])
                .SingleOrDefault();

            var response = new CommandResponse { Message = "Updated interwiki entry." }; 
            
            if (existing == null)
            {
                existing = new InterwikiPrefix{Prefix = this.Arguments[0]};
                response.Message = "Created new interwiki entry.";
            }

            existing.Url = this.Arguments[1];

            this.session.SaveOrUpdate(existing);
            this.session.Flush();
            yield return response;
        }
        
        [Help("<interwiki>", "Removes the specified interwiki entry")]
        [SubcommandInvocation("delete")]
        [SubcommandInvocation("rm")]
        [SubcommandInvocation("remove")]
        [RequiredArguments(1)]
        protected IEnumerable<CommandResponse> Delete()
        {
            var existing = this.session.QueryOver<InterwikiPrefix>()
                .Where(x => x.Prefix == this.Arguments[0])
                .SingleOrDefault();

            var response = new CommandResponse { Message = "Deleted interwiki entry." }; 
            
            if (existing == null)
            {
                response.Message = "Nothing found to delete.";
                return new[] { response };
            }

            this.session.Delete(existing);
            this.session.Flush();
            return new[] { response };
        }
        
        [Help("", "Imports all interwiki prefixes from the active MediaWiki site. Any new entries will be automatically added; any updated or deleted entries will be held for review.")]
        [SubcommandInvocation("import")]
        [Undocumented]
        protected IEnumerable<CommandResponse> Import()
        {
            yield break;
        }
        
        [Help("<imported name>", "Accepts a held imported entry as correct")]
        [SubcommandInvocation("accept")]
        [RequiredArguments(1)]
        protected IEnumerable<CommandResponse> Accept()
        {
            var imported = this.session.QueryOver<InterwikiPrefix>()
                .Where(x => x.Prefix == this.Arguments[0])
                .SingleOrDefault();
            
            var existing = this.session.QueryOver<InterwikiPrefix>()
                .Where(x => x.Prefix == imported.ImportedAs)
                .SingleOrDefault();

            if (existing == null)
            {
                imported.Prefix = imported.ImportedAs;
                imported.ImportedAs = null;
                this.session.Update(imported);
            }
            else
            {
                existing.Url = imported.Url;
                this.session.Update(existing);
                this.session.Delete(imported);
            }
            
            this.session.Flush();
            
            return new[] { new CommandResponse { Message = "Accepted interwiki entry." } };
        }
        
        [Help("", "Removes any markers for interwikis missing from the last import.")]
        [SubcommandInvocation("forgetmissing")]
        protected IEnumerable<CommandResponse> ForgetMissing()
        {
            var absentMarked = this.session.QueryOver<InterwikiPrefix>()
                .Where(x => x.AbsentFromLastImport == true)
                .List();

            foreach (var prefix in absentMarked)
            {
                prefix.AbsentFromLastImport = false;
                this.session.Update(prefix);
            }
            this.session.Flush();
            
            return new[] { new CommandResponse { Message = "Done." } };
        }
    }
}