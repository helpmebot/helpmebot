namespace Helpmebot.ChannelServices.Commands.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.Configuration;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("basewiki")]
    [CommandFlag(Flags.Info)]
    [HelpSummary("Configures which wiki is used by the bot for commands which pull data from the wiki.")]
    public class BaseWikiCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly IChannelManagementService channelManagementService;
        private readonly MediaWikiSiteConfiguration mwConfig;

        public BaseWikiCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder,
            IChannelManagementService channelManagementService,
            MediaWikiSiteConfiguration mwConfig) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
            this.channelManagementService = channelManagementService;
            this.mwConfig = mwConfig;
        }

        [Help("", "Retrieves the base wiki this channel is configured to use")]
        [CommandParameter("target=", "The target channel to apply this command to", "target", typeof(string))]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var target = this.Parameters.GetParameter("target", this.CommandSource);
            
            try
            {
                var mediaWikiSite = this.channelManagementService.GetBaseWiki(target);
                var wikiSite = this.mwConfig.GetSite(mediaWikiSite, true);

                return this.responder.Respond(
                    "channelservices.command.basewiki",
                    this.CommandSource,
                    new object[] { target, wikiSite.WikiId, wikiSite.Api });
            }
            catch (NullReferenceException)
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, target));
            }
        }

        [Help("", "Lists all the wikis the bot is configured to use")]
        [SubcommandInvocation("list")]
        [CommandFlag(Flags.Protected)]
        protected IEnumerable<CommandResponse> List()
        {
            Func<MediaWikiSiteConfiguration.MediaWikiSite, string> item = x => this.responder.GetMessagePart(
                "channelservices.command.basewiki.wikis.item",
                this.CommandSource,
                new object[] { x.WikiId, x.Api });

            return this.mwConfig.Sites.Select(x => new CommandResponse { Message = item(x) });
        }

        [Help("<wiki>", "Sets the base wiki for this channel. Use the \"database\" name of the wiki, eg `enwiki` or `commonswiki`.")]
        [SubcommandInvocation("set")]
        [CommandParameter("target=", "The target channel to apply this command to", "target", typeof(string))]
        [RequiredArguments(1)]
        [CommandFlag(Flags.LocalConfiguration)]
        [CommandFlag(Flags.Configuration, true)]
        protected IEnumerable<CommandResponse> Set()
        {
            var target = this.Parameters.GetParameter("target", this.CommandSource);
            
            try
            {
                var wiki = this.mwConfig.Sites.FirstOrDefault(x => x.WikiId== this.Arguments[0]);

                if (wiki == null)
                {
                    return this.responder.Respond(
                        "channelservices.command.basewiki.notfound",
                        this.CommandSource,
                        this.Arguments[0]);
                }
                
                this.channelManagementService.SetBaseWiki(target, wiki.WikiId);

                return this.responder.Respond(
                    "channelservices.command.basewiki.set",
                    this.CommandSource,
                    new object[] { target, wiki.WikiId, wiki.Api });
            }
            catch (NullReferenceException)
            {
                throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, target));
            }

        }
    }
}