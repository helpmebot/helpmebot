namespace Helpmebot.CoreServices.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Castle.Core.Internal;
    using Helpmebot.Attributes;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Attributes;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class HelpSyncService : IHelpSyncService
    {
        private readonly IMediaWikiApiHelper apiHelper;
        private readonly ICommandParser commandParser;
        private readonly BotConfiguration botConfiguration;
        private readonly MediaWikiDocumentationConfiguration docConfig;

        public HelpSyncService(
            IMediaWikiApiHelper apiHelper,
            ICommandParser commandParser,
            BotConfiguration botConfiguration,
            MediaWikiDocumentationConfiguration docConfig)
        {
            this.apiHelper = apiHelper;
            this.commandParser = commandParser;
            this.botConfiguration = botConfiguration;
            this.docConfig = docConfig;
        }

        public void DoSync(IUser forUser)
        {
            var botService = this.apiHelper.GetApi(
                this.docConfig.MediaWikiApiEndpoint,
                this.docConfig.MediaWikiApiUsername,
                this.docConfig.MediaWikiApiPassword);

            botService.Login();

            var registrationInfo = this.commandParser.GetCommandRegistrations();
            var pagesInPrefix = botService.PrefixSearch(this.docConfig.DocumentationPrefix).ToList();
            var editSummary = string.Format("Updating documentation for {0}", forUser);

            // collect all the types from the command registrations
            var types = new HashSet<Type>();
            foreach (var registrationInfoValue in registrationInfo.Values)
            {
                foreach (var reg in registrationInfoValue.Keys)
                {
                    types.Add(reg.Type);
                }
            }

            // produce a help page for each of them
            var commandList = new List<CommandListEntry>();
            foreach (var commandClass in types)
            {
                string pageName, canonicalName, mainFlags, category;
                List<string> aliases;

                var pageContent = this.CreateHelpPage(
                    commandClass,
                    out pageName,
                    out canonicalName,
                    out aliases,
                    out mainFlags,
                    out category);

                if (pageContent == null)
                {
                    continue;
                }

                botService.WritePage(
                    pageName,
                    pageContent,
                    editSummary,
                    null,
                    true,
                    false);

                pagesInPrefix.Remove(pageName);

                commandList.Add(new CommandListEntry(pageName, canonicalName, canonicalName, mainFlags, category));
                foreach (var alias in aliases)
                {
                    commandList.Add(new CommandListEntry(pageName, alias, canonicalName, mainFlags, category));
                }
            }

            var clBuilder = new StringBuilder("{| class=\"wikitable\"\n!Command\n!Help page\n");
            var allHelpBuilder = new StringBuilder();
            var catMap = new Dictionary<string, StringBuilder>();
            foreach (var entry in commandList.OrderBy(x => x.Alias))
            {
                clBuilder.Append("|-\n");
                clBuilder.AppendFormat(
                        "|{0}{1} || [[{2}|{3}]]",
                        this.botConfiguration.CommandTrigger,
                        entry.Alias,
                        entry.PageName,
                        entry.Canonical)
                    .AppendLine();

                if (entry.Alias == entry.Canonical)
                {
                    allHelpBuilder.AppendFormat("{{{{{0}}}}}", entry.PageName).AppendLine();

                    if (!catMap.ContainsKey(entry.Category))
                    {
                        catMap.Add(entry.Category, new StringBuilder());
                    }

                    catMap[entry.Category].AppendFormat("{{{{{0}}}}}", entry.PageName).AppendLine();
                }
            }

            clBuilder.Append("|}");

            botService.WritePage(
                this.docConfig.DocumentationPrefix + "CommandList",
                clBuilder.ToString(),
                editSummary,
                null,
                true,
                false);
            botService.WritePage(
                this.docConfig.DocumentationPrefix + "All",
                allHelpBuilder.ToString(),
                editSummary,
                null,
                true,
                false);

            pagesInPrefix.Remove(this.docConfig.DocumentationPrefix + "CommandList");
            pagesInPrefix.Remove(this.docConfig.DocumentationPrefix + "All");

            foreach (var pair in catMap)
            {
                botService.WritePage(
                    this.docConfig.DocumentationPrefix + pair.Key,
                    pair.Value.ToString(),
                    editSummary,
                    null,
                    true,
                    false);
                pagesInPrefix.Remove(this.docConfig.DocumentationPrefix + pair.Key);
            }

            // delete the excess pages
            foreach (var p in pagesInPrefix)
            {
                botService.DeletePage(p, editSummary);
            }

            this.apiHelper.Release(botService);
        }

        private string CreateHelpPage(
            Type commandClass,
            out string pageName,
            out string canonicalName,
            out List<string> aliases,
            out string mainFlags,
            out string category)
        {
            var undocumentedAttribute =
                commandClass.GetCustomAttributes(typeof(UndocumentedAttribute), false);
            if (undocumentedAttribute.Length > 0)
            {
                canonicalName = null;
                pageName = null;
                aliases = null;
                mainFlags = null;
                category = null;
                return null;
            }
            
            var commandInvocationAttribute =
                commandClass.GetCustomAttributes(typeof(CommandInvocationAttribute), false);
            if (commandInvocationAttribute.Length == 0)
            {
                canonicalName = null;
                pageName = null;
                aliases = null;
                mainFlags = null;
                category = null;
                return null;
            }

            string helpSummary = null;
            var helpSummaryAttribute =
                commandClass.GetCustomAttributes(typeof(HelpSummaryAttribute), false);
            if (helpSummaryAttribute.Length == 1)
            {
                helpSummary = ((HelpSummaryAttribute)helpSummaryAttribute.First()).Description;
            }

            var canonName = canonicalName =
                ((CommandInvocationAttribute) commandInvocationAttribute.First()).CommandName;
            aliases = commandInvocationAttribute.Select(x => ((CommandInvocationAttribute) x).CommandName)
                .Where(x => x != canonName)
                .ToList();

            var helpCat = commandClass.GetCustomAttributes(typeof(HelpCategoryAttribute), true)
                .Select(x => (HelpCategoryAttribute) x)
                .FirstOrDefault();
            category = helpCat == null ? "Default" : helpCat.Category;

            var cmdFlagAttrs = commandClass.GetCustomAttributes(typeof(CommandFlagAttribute), false)
                .Select(x => (CommandFlagAttribute) x);
            var globalOnlyAbbrSup = "<sup><abbr title=\"Global only\">†</abbr></sup>";
            var flagFormat = "[[Flags#{0}|{0}]]{1}";
            mainFlags = string.Join(
                " or ",
                cmdFlagAttrs.Select(
                    i => string.Format(flagFormat, i.Flag, i.GlobalOnly ? globalOnlyAbbrSup : string.Empty)));

            var methodInfos = commandClass.GetMethods(
                BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);

            var pageBuilder = new StringBuilder();
            pageBuilder.AppendFormat("== {0} ==", canonicalName).AppendLine();
            pageBuilder.AppendFormat("* Requires flag: {0}", mainFlags).AppendLine();

            if (aliases.Count > 0)
            {
                pageBuilder.AppendFormat("* Also invokable as: {0}", string.Join(", ", aliases)).AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(helpSummary))
            {
                pageBuilder.AppendFormat(helpSummary).AppendLine();
            }
            
            pageBuilder.AppendLine().AppendLine(";Syntax").AppendLine();

            foreach (var info in methodInfos)
            {
                if (info.IsAbstract || info.IsConstructor || info.IsPrivate)
                {
                    continue;
                }

                var invokAttr = info.GetAttribute<SubcommandInvocationAttribute>();
                if (invokAttr == null && info.Name != "Execute")
                {
                    continue;
                }

                var helpAttr = info.GetAttribute<HelpAttribute>();
                if (helpAttr == null)
                {
                    continue;
                }

                string subCommandName = canonicalName;
                if (info.Name != "Execute")
                {
                    subCommandName += " " + invokAttr.CommandName;
                }

                foreach (var syntax in helpAttr.HelpMessage.Syntax)
                {
                    pageBuilder.AppendFormat(
                        "* <tt>{0}{1} {2}</tt>\n",
                        this.botConfiguration.CommandTrigger,
                        subCommandName,
                        syntax);
                }

                var subcmdFlagAttrs = info.GetCustomAttributes(typeof(CommandFlagAttribute), false)
                    .Select(x => (CommandFlagAttribute) x);
                var subFlags = string.Join(
                    " or ",
                    subcmdFlagAttrs.Select(
                        i => string.Format(flagFormat, i.Flag, i.GlobalOnly ? globalOnlyAbbrSup : string.Empty)));

                if (!string.IsNullOrWhiteSpace(subFlags))
                {
                    pageBuilder.AppendFormat("*:Requires flags: {0}\n", subFlags);
                }

                foreach (var text in helpAttr.HelpMessage.Text)
                {
                    pageBuilder.AppendFormat("*:{0}\n", text);
                }
            }

            pageBuilder.AppendLine();
            pageBuilder.AppendLine(
                "{{#ifexist:" + this.docConfig.HumanDocumentationPrefix + canonicalName + "|{{"
                + this.docConfig.HumanDocumentationPrefix + canonicalName + "}}|}}");

            pageName = this.docConfig.DocumentationPrefix + "Command/" + canonicalName;
            return pageBuilder.ToString();
        }

        private class CommandListEntry
        {
            public CommandListEntry(string pageName, string alias, string canonical, string mainFlags, string category)
            {
                this.PageName = pageName;
                this.Alias = alias;
                this.Canonical = canonical;
                this.MainFlags = mainFlags;
                this.Category = category;
            }

            public string PageName { get; private set; }
            public string Alias { get; private set; }
            public string Canonical { get; private set; }
            public string MainFlags { get; private set; }
            public string Category { get; private set; }
        }
    }
}