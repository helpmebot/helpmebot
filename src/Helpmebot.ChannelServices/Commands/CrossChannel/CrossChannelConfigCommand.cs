namespace Helpmebot.ChannelServices.Commands.CrossChannel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using NDesk.Options;
    using NHibernate;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("crosschannel")]
    [CommandFlag(Flags.Configuration, true)]
    public class CrossChannelConfigCommand : CommandBase
    {
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
            ISession databaseSession) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
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
                    throw new Exception("Cannot find configuration for the current channel!");
                }
                
                if (frontend == null)
                {
                    throw new Exception("Cannot find configuration for the specified channel!");
                }

                this.crossChannelService.Configure(frontend, backend, this.databaseSession);
                
                this.databaseSession.Transaction.Commit();
                
                return new[] {new CommandResponse {Message = "Done."}};
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
                    throw new Exception("Cannot find configuration for the current channel!");
                }
                
                this.crossChannelService.Deconfigure(backend, this.databaseSession);
                
                this.databaseSession.Transaction.Commit();
                
                return new[] {new CommandResponse {Message = "Done."}};
            }
            catch(Exception ex)
            {
                this.databaseSession.Transaction.Rollback();
                throw new CommandErrorException(ex.Message, ex);
            }
        }
        
        [SubcommandInvocation("notify")]
        [RequiredArguments(1)]
        [Help(new[]{"--enable","--disable"}, "Enables or disables notifications")]
        protected IEnumerable<CommandResponse> NotifyMode()
        {
            var status = (bool?)null;
            var opts = new OptionSet
            {
                {"enable", x => status = true},
                {"disable", x => status = false}
            };
            opts.Parse(this.Arguments);

            if (!status.HasValue)
            {
                throw new CommandInvocationException("notify");
            }
            
            this.databaseSession.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                var backend = this.databaseSession.GetChannelObject(this.CommandSource);

                if (backend == null)
                {
                    throw new Exception("Cannot find configuration for the current channel!");
                }
                
                this.crossChannelService.NotificationStatus(backend, status.Value, this.databaseSession);
                
                this.databaseSession.Transaction.Commit();
                
                return new[] {new CommandResponse {Message = "Done."}};
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
                    throw new Exception("Cannot find configuration for the current channel!");
                }
                
                this.crossChannelService.NotificationKeyword(backend, this.Arguments.First(), this.databaseSession);
                
                this.databaseSession.Transaction.Commit();
                
                return new[] {new CommandResponse {Message = "Done."}};
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
                    throw new Exception("Cannot find configuration for the current channel!");
                }

                this.crossChannelService.NotificationMessage(
                    backend,
                    string.Join(" ", this.Arguments),
                    this.databaseSession);
                
                this.databaseSession.Transaction.Commit();
                
                return new[] {new CommandResponse {Message = "Done."}};
            }
            catch(Exception ex)
            {
                this.databaseSession.Transaction.Rollback();
                throw new CommandErrorException(ex.Message, ex);
            }
        }
        
        
    }
}