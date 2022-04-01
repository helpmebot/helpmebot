namespace Helpmebot.Commands.Commands.BotManagement
{
    using System.Collections.Generic;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
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
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly IResponder responder;
        private readonly IInterwikiService interwikiService;

        public InterwikiCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession session,
            IMediaWikiApiHelper apiHelper,
            IResponder responder,
            IInterwikiService interwikiService) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.session = session;
            this.apiHelper = apiHelper;
            this.responder = responder;
            this.interwikiService = interwikiService;
        }
        
        [Help("<interwiki> <url>", "Adds or updates the specified interwiki entry")]
        [SubcommandInvocation("add")]
        [SubcommandInvocation("insert")]
        [SubcommandInvocation("edit")]
        [SubcommandInvocation("update")]
        [RequiredArguments(2)]
        protected IEnumerable<CommandResponse> Add()
        {
            this.interwikiService.AddOrUpdate(this.Arguments[0], this.Arguments[1], out var updated);
            
            var key = "commands.command.iw.updated";
            if (!updated)
            {
                key = "commands.command.iw.created";
            }

            return this.responder.Respond(key, this.CommandSource);
        }
        
        [Help("<interwiki>", "Removes the specified interwiki entry")]
        [SubcommandInvocation("delete")]
        [SubcommandInvocation("del")]
        [SubcommandInvocation("rm")]
        [SubcommandInvocation("remove")]
        [RequiredArguments(1)]
        protected IEnumerable<CommandResponse> Delete()
        {
            if (this.interwikiService.Delete(this.Arguments[0]))
            {
                return this.responder.Respond("commands.command.iw.deleted", this.CommandSource);
            }
            
            return this.responder.Respond("commands.command.iw.delete-not-found", this.CommandSource);
        }
        
        [Help("", "Imports all interwiki prefixes from the active MediaWiki site. Any new entries will be automatically added; any updated or deleted entries will be held for review.")]
        [SubcommandInvocation("import")]
        protected IEnumerable<CommandResponse> Import()
        {
            var mediaWikiSiteObject = this.session.GetMediaWikiSiteObject(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSiteObject);

            var (upToDateIw, createdIw, updatedIw, deletedIw) = this.interwikiService.Import(mediaWikiApi);

            return this.responder.Respond(
                "commands.command.iw.imported",
                this.CommandSource,
                new object[]
                {
                    upToDateIw, createdIw, updatedIw, deletedIw
                });
        }
        
        [Help("<prefix>", "Accepts a held modification of an entry as correct")]
        [SubcommandInvocation("accept")]
        [RequiredArguments(1)]
        protected IEnumerable<CommandResponse> Accept()
        {
            var accepted = this.interwikiService.Accept(this.Arguments[0]);

            if (!accepted)
            {
                return this.responder.Respond("commands.command.iw.accept-not-found", this.CommandSource);
            }            

            return this.responder.Respond("commands.command.iw.accepted", this.CommandSource);
        }
        
        [Help("<prefix>", "Rejects a held modification of an entry")]
        [SubcommandInvocation("reject")]
        [RequiredArguments(1)]
        protected IEnumerable<CommandResponse> Reject()
        {
            var rejected = this.interwikiService.Reject(this.Arguments[0]);

            if (!rejected)
            {
                return this.responder.Respond("commands.command.iw.reject-not-found", this.CommandSource);
            }            

            return this.responder.Respond("commands.command.iw.rejected", this.CommandSource);
        }
        
        [Help("", "Removes any markers for interwikis created or missing from the last import.")]
        [SubcommandInvocation("forgetmissing")]
        protected IEnumerable<CommandResponse> ForgetMissing()
        {
            this.interwikiService.ForgetMissing();
            return this.responder.Respond("common.done", this.CommandSource);
        }
    }
}