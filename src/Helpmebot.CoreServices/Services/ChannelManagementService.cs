namespace Helpmebot.CoreServices.Services
{
    using System;
    using System.Data;
    using System.Linq;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Models;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;

    public class ChannelManagementService : IChannelManagementService, ISilentModeConfiguration
    {
        private readonly ISession session;
        private readonly IIrcClient client;
        private readonly IFlagService flagService;
        private readonly IAccessLogService accessLogService;
        private readonly ILogger logger;

        public ChannelManagementService(
            ISession session,
            IIrcClient client,
            IFlagService flagService,
            IAccessLogService accessLogService,
            ILogger logger,
            ICommandHandler commandHandler
        )
        {
            this.session = session;
            this.client = client;
            this.flagService = flagService;
            this.accessLogService = accessLogService;
            this.logger = logger;

            this.client.InviteReceivedEvent += this.OnInvite;
            this.client.WasKickedEvent += this.OnKicked;
            
            commandHandler.SilentModeConfiguration = this;
        }

        public void JoinChannel(string channelName)
        {
            using (var txn = this.session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var channel = this.session.CreateCriteria<Channel>()
                    .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                    .List<Channel>()
                    .FirstOrDefault();

                if (channel == null)
                {
                    var mediaWikiSite = this.session.CreateCriteria<MediaWikiSite>()
                        .Add(Restrictions.Eq(nameof(MediaWikiSite.IsDefault), true))
                        .List<MediaWikiSite>()
                        .FirstOrDefault();

                    channel = new Channel
                    {
                        Name = channelName,
                        Enabled = true,
                        BaseWiki = mediaWikiSite
                    };
                }

                channel.Enabled = true;

                this.session.SaveOrUpdate(channel);
                txn.Commit();
            }

            this.client.JoinChannel(channelName);
        }

        public void PartChannel(string channelName, string message)
        {
            using (var txn = this.session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var channel = this.session.CreateCriteria<Channel>()
                    .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                    .List<Channel>()
                    .FirstOrDefault();

                this.client.PartChannel(channelName, message);

                if (channel == null)
                {
                    return;
                }

                channel.Enabled = false;

                this.session.SaveOrUpdate(channel);
                txn.Commit();
            }
        }

        public void OnInvite(object sender, InviteEventArgs e)
        {
            const string FlagRequired = Flags.BotManagement;

            this.flagService.GetFlagsForUser(e.User, e.Channel);
            CommandAclStatus aclStatus;

            var userIsAllowed = this.flagService.UserHasFlag(e.User, FlagRequired, e.Channel);
            if (!userIsAllowed)
            {
                aclStatus = CommandAclStatus.DeniedMain;
            }
            else
            {
                this.JoinChannel(e.Channel);
                aclStatus = CommandAclStatus.Allowed;
            }

            this.accessLogService.SaveLogEntry(
                this.GetType(),
                "INVITE " + e.Channel,
                e.User,
                "INVITE",
                FlagRequired,
                null,
                aclStatus);
        }

        public void OnKicked(object sender, KickedEventArgs e)
        {
            using (var txn = this.session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var channel = this.session.CreateCriteria<Channel>()
                    .Add(Restrictions.Eq(nameof(Channel.Name), e.Channel))
                    .List<Channel>()
                    .FirstOrDefault();

                if (channel == null)
                {
                    return;
                }

                channel.Enabled = false;

                this.session.Save(channel);
                txn.Commit();
            }
        }

        public void ConfigureAutolink(string channelName, bool state)
        {
            using (var txn = this.session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    var channel = this.session.CreateCriteria<Channel>()
                        .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                        .List<Channel>()
                        .FirstOrDefault();

                    if (channel == null)
                    {
                        throw new NullReferenceException("Channel object not found");
                    }

                    channel.AutoLink = state;

                    this.session.SaveOrUpdate(channel);
                    txn.Commit();
                    this.session.Flush();
                }
                finally
                {
                    if (txn.IsActive)
                    {
                        txn.Rollback();
                    }
                }
            }
        }

        public void ConfigureSilence(string channelName, bool state)
        {
            using (var txn = this.session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    var channel = this.session.CreateCriteria<Channel>()
                        .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                        .List<Channel>()
                        .FirstOrDefault();

                    if (channel == null)
                    {
                        throw new NullReferenceException("Channel object not found");
                    }

                    channel.Silenced = state;

                    this.session.SaveOrUpdate(channel);
                    txn.Commit();
                    this.session.Flush();
                }
                finally
                {
                    if (txn.IsActive)
                    {
                        txn.Rollback();
                    }
                }
            }
        }

        public void ConfigureFunCommands(string channelName, bool disabled)
        {
            using (var txn = this.session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    var channel = this.session.CreateCriteria<Channel>()
                        .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                        .List<Channel>()
                        .FirstOrDefault();

                    if (channel == null)
                    {
                        throw new NullReferenceException("Channel object not found");
                    }

                    channel.HedgehogMode = disabled;

                    this.session.SaveOrUpdate(channel);
                    txn.Commit();
                    this.session.Flush();
                }
                finally
                {
                    if (txn.IsActive)
                    {
                        txn.Rollback();
                    }
                }
            }
        }

        public bool FunCommandsDisabled(string channelName)
        {
            var channel = this.session.CreateCriteria<Channel>()
                .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                .List<Channel>()
                .FirstOrDefault();

            if (channel == null)
            {
                return true;
            }

            return channel.HedgehogMode;
        }

        public bool AutoLinkEnabled(string channelName)
        {
            var channel = this.session.CreateCriteria<Channel>()
                .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                .List<Channel>()
                .FirstOrDefault();

            if (channel == null)
            {
                return false;
            }

            return channel.AutoLink;
        }

        public bool IsSilenced(string channelName)
        {
            var channel = this.session.CreateCriteria<Channel>()
                .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                .List<Channel>()
                .FirstOrDefault();

            if (channel == null)
            {
                return false;
            }

            return channel.Silenced;
        }

        public MediaWikiSite GetBaseWiki(string channelName)
        {
            var channel = this.session.CreateCriteria<Channel>()
                .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                .List<Channel>()
                .FirstOrDefault();

            if (channel == null)
            {
                throw new NullReferenceException("Channel object not found");
            }

            return channel.BaseWiki;
        }

        [Obsolete]
        public Channel GetChannel(string channelName)
        {
            return this.session.CreateCriteria<Channel>()
                .Add(Restrictions.Eq(nameof(Channel.Name), channelName))
                .List<Channel>()
                .FirstOrDefault();
        }

        bool ISilentModeConfiguration.BotIsSilent(string destination, CommandMessage message)
        {
            if (!destination.StartsWith("#"))
            {
                return false;
            }

            try
            {
                return this.IsSilenced(destination);
            }
            catch (Exception ex)
            {
                this.logger.ErrorFormat(ex, "Error encountered determining silence configuration for {0}", destination);
                return false;
            }
        }
    }
}