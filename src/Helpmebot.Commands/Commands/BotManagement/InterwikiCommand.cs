namespace Helpmebot.Commands.Commands.BotManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
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
        private readonly IMediaWikiApiHelper apiHelper;

        public InterwikiCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ISession session,
            IMediaWikiApiHelper apiHelper) : base(
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
        protected IEnumerable<CommandResponse> Import()
        {
            this.Logger.Debug("Fetching existing prefixes");
            var allPrefixes = this.session.QueryOver<InterwikiPrefix>().List().ToDictionary(x => x.Prefix);
            
            this.Logger.Debug("Marking existing as absent");
            foreach (var prefix in allPrefixes)
            {
                prefix.Value.AbsentFromLastImport = true;
            }
            
            this.Logger.Debug("Downloading latest map");
            var mediaWikiSiteObject = this.session.GetMediaWikiSiteObject(this.CommandSource);
            var mediaWikiApi = this.apiHelper.GetApi(mediaWikiSiteObject);
            var prefixesToImport = mediaWikiApi.GetInterwikiPrefixes().ToList();
            
            int deletedIw = 0, createdIw = 0, updatedIw = 0, upToDateIw = 0;
            
            this.Logger.Debug("Iterating import");
            int i = 0;
            foreach (var prefix in prefixesToImport)
            {
                this.Logger.DebugFormat(
                    "  processed {0} of {1} - {2}%",
                    i,
                    prefixesToImport.Count,
                    i / prefixesToImport.Count * 100);
                i++;
                
                if (allPrefixes.ContainsKey(prefix.Prefix))
                {
                    allPrefixes[prefix.Prefix].AbsentFromLastImport = false;
                    
                    // present, are we up-to-date?
                    if (allPrefixes[prefix.Prefix].Url == prefix.Url)
                    {
                        // yes
                        upToDateIw++;
                        continue;
                    }

                    var existingImport = this.session.QueryOver<InterwikiPrefix>().Where(x => x.ImportedAs == prefix.Prefix).SingleOrDefault();
                    if (existingImport == null)
                    {
                        existingImport = new InterwikiPrefix();
                    }

                    existingImport.Prefix = prefix.Prefix + "-import";
                    existingImport.ImportedAs = prefix.Prefix;
                    existingImport.Url = prefix.Url;
                    existingImport.AbsentFromLastImport = false;
                    this.session.SaveOrUpdate(existingImport);
                    updatedIw++;
                }
                else
                {
                    this.session.Save(
                        new InterwikiPrefix
                        {
                            Prefix = prefix.Prefix, Url = prefix.Url,
                            AbsentFromLastImport = false, CreatedSinceLast = true
                        });
                    createdIw++;
                }
            }
            
            this.Logger.Debug("Saving absent prefixes");
            foreach (var prefix in allPrefixes.Where(x => x.Value.AbsentFromLastImport))
            {
                deletedIw++;
                this.session.Update(prefix.Value);
            }
            
            this.Logger.Debug("Flushing session");
            this.session.Flush();

            return new[]
            {
                new CommandResponse
                {
                    Message = $"Import complete. {upToDateIw} up-to-date, {createdIw} created, {updatedIw} updated for review, {deletedIw} deleted for review."
                }
            };
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
        
        [Help("", "Removes any markers for interwikis created or missing from the last import.")]
        [SubcommandInvocation("forgetmissing")]
        protected IEnumerable<CommandResponse> ForgetMissing()
        {
            var absentMarked = this.session.QueryOver<InterwikiPrefix>()
                .Where(x => x.AbsentFromLastImport || x.CreatedSinceLast)
                .List();

            foreach (var prefix in absentMarked)
            {
                prefix.AbsentFromLastImport = false;
                prefix.CreatedSinceLast = false;
                this.session.Update(prefix);
            }
            this.session.Flush();
            
            return new[] { new CommandResponse { Message = "Done." } };
        }
    }
}