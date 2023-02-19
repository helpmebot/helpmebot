namespace Helpmebot.ChannelServices.Commands.CrossChannel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Mono.Options;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("crosschannel")]
    [CommandFlag(Flags.Configuration, true)]
    public class CrossChannelConfigCommand : CommandBase
    {
        private readonly IResponder responder;
        private readonly ICrossChannelService crossChannelService;
        private readonly ISession databaseSession;

        public CrossChannelConfigCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            ICrossChannelService crossChannelService,
            ISession databaseSession,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
            this.crossChannelService = crossChannelService;
            this.databaseSession = databaseSession;
        }

        [RequiredArguments(1)]
        [SubcommandInvocation("configure")]
        [Help("<channel>", "Sets up cross-channel notifications from the provided channel to this channel.")]
        protected IEnumerable<CommandResponse> ConfigureMode()
        {
            this.databaseSession.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                var backend = this.databaseSession.GetChannelObject(this.CommandSource);
                var frontend = this.databaseSession.GetChannelObject(this.Arguments.First());

                if (backend == null)
                {
                    throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
                }
                
                if (frontend == null)
                {
                    throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.Arguments.First()));
                }

                this.crossChannelService.Configure(frontend.Name, backend.Name);
                
                this.databaseSession.Transaction.Commit();
                
                return this.responder.Respond("common.done", this.CommandSource);
            }
            catch(Exception ex)
            {
                this.databaseSession.Transaction.Rollback();
                throw new CommandErrorException(ex.Message, ex);
            }
        }
        
        [Help("", "Removes cross-channel notification configuration from this channel.")]
        [SubcommandInvocation("deconfigure")]
        protected IEnumerable<CommandResponse> DeconfigureMode()
        {
            this.databaseSession.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                var backend = this.databaseSession.GetChannelObject(this.CommandSource);

                if (backend == null)
                {
                    throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
                }
                
                this.crossChannelService.Deconfigure(backend.Name);
                
                this.databaseSession.Transaction.Commit();
                
                return this.responder.Respond("common.done", this.CommandSource);
            }
            catch(Exception ex)
            {
                this.databaseSession.Transaction.Rollback();
                throw new CommandErrorException(ex.Message, ex);
            }
        }
        
        [SubcommandInvocation("notifications")]
        [SubcommandInvocation("notify")]
        [Help("", "Manages the notification state. Without parameters, this will return the current state.")]
        [CommandParameter("enable", "Enable notifications", "enable", typeof(bool), hidden: true)]
        [CommandParameter("disable", "Disable notifications", "enable", typeof(bool), booleanInverse: true, hidden: true)]
        protected IEnumerable<CommandResponse> NotifyMode()
        {
            var status = this.Parameters.GetParameter("enable", (bool?)null);
            
            this.databaseSession.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                var backend = this.databaseSession.GetChannelObject(this.CommandSource);

                if (backend == null)
                {
                    throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
                }
                
                if (!status.HasValue)
                {
                    var currentStatus = this.crossChannelService.GetNotificationStatus(backend.Name);
                    this.databaseSession.Transaction.Rollback();
                    
                    return this.responder.Respond(
                        "channelservices.command.crosschannel." + (currentStatus ? "enabled" : "disabled"),
                        this.CommandSource,
                        backend.Name);
                }
                else
                {
                    this.crossChannelService.SetNotificationStatus(backend.Name, status.Value);

                    this.databaseSession.Transaction.Commit();
                
                    return this.responder.Respond("common.done", this.CommandSource);
                }
            }
            catch(Exception ex)
            {
                this.databaseSession.Transaction.Rollback();
                throw new CommandErrorException(ex.Message, ex);
            }
        }
        
        [SubcommandInvocation("notifykeyword")]
        [Help("<keyword>", "Sets the keyword used for triggering the notification")]
        [RequiredArguments(1)]
        protected IEnumerable<CommandResponse> NotifyKeywordMode()
        {
            this.databaseSession.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                var backend = this.databaseSession.GetChannelObject(this.CommandSource);

                if (backend == null)
                {
                    throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
                }
                
                this.crossChannelService.SetNotificationKeyword(backend.Name, this.Arguments.First());
                
                this.databaseSession.Transaction.Commit();
                
                return this.responder.Respond("common.done", this.CommandSource);
            }
            catch(Exception ex)
            {
                this.databaseSession.Transaction.Rollback();
                throw new CommandErrorException(ex.Message, ex);
            }
        }
        
        [SubcommandInvocation("notifymessage")]
        [Help("<message>", "Sets the message used for the notification")]
        [RequiredArguments(1)]
        protected IEnumerable<CommandResponse> NotifyMessageMode()
        {
            this.databaseSession.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                var backend = this.databaseSession.GetChannelObject(this.CommandSource);

                if (backend == null)
                {
                    throw new CommandErrorException(this.responder.GetMessagePart("common.channel-not-found", this.CommandSource, this.CommandSource));
                }

                this.crossChannelService.SetNotificationMessage(
                    backend.Name,
                    string.Join(" ", this.Arguments));
                
                this.databaseSession.Transaction.Commit();
                
                return this.responder.Respond("common.done", this.CommandSource);
            }
            catch(Exception ex)
            {
                this.databaseSession.Transaction.Rollback();
                throw new CommandErrorException(ex.Message, ex);
            }
        }
        
        
    }
}