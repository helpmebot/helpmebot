namespace Helpmebot.ChannelServices.Services
{
    using System;
    using System.Data;
    using System.Linq;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Models;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Events;
    using Stwalkerster.IrcClient.Interfaces;

    public class ChannelManagementService : IChannelManagementService
    {
        private readonly ISession session;
        private readonly IIrcClient client;
        private readonly IFlagService flagService;
        private readonly IAccessLogService accessLogService;

        public ChannelManagementService(
            ISession session,
            IIrcClient client,
            IFlagService flagService,
            IAccessLogService accessLogService)
        {
            this.session = session;
            this.client = client;
            this.flagService = flagService;
            this.accessLogService = accessLogService;

            this.client.InviteReceivedEvent += this.OnInvite;
            this.client.WasKickedEvent += this.OnKicked;
        }

        public void JoinChannel(string channelName)
        {
            using (var txn = this.session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var channel = this.session.CreateCriteria<Channel>()
                    .Add(Restrictions.Eq("Name", channelName))
                    .List<Channel>()
                    .FirstOrDefault();

                if (channel == null)
                {

                    var mediaWikiSite = this.session.CreateCriteria<MediaWikiSite>()
                        .Add(Restrictions.Eq("IsDefault", true))
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
                    .Add(Restrictions.Eq("Name", channelName))
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
                    .Add(Restrictions.Eq("Name", e.Channel))
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
                        .Add(Restrictions.Eq("Name", channelName))
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
                        .Add(Restrictions.Eq("Name", channelName))
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
        
        public void ConfigureFunCommands(string channelName, bool state)
        {            
            var channel = this.GetChannel(channelName);

            if (channel == null)
            {
                throw new NullReferenceException("Channel object not found");
            }
            
            channel.HedgehogMode = state;
            this.session.SaveOrUpdate(channelName);
            this.session.Flush();
        }

        [Obsolete]
        public Channel GetChannel(string channelName)
        {
            return this.session.CreateCriteria<Channel>()
                .Add(Restrictions.Eq("Name", channelName))
                .List<Channel>()
                .FirstOrDefault();
        }
        
    }
}