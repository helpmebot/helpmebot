namespace Helpmebot.Services
{
    using System.Linq;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
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
        }

        public void JoinChannel(string channelName, ISession localSession)
        {
            var channel = localSession.CreateCriteria<Channel>()
                .Add(Restrictions.Eq("Name", channelName))
                .List<Channel>()
                .FirstOrDefault();

            if (channel == null)
            {
                channel = new Channel
                {
                    Name = channelName,
                    Enabled = true
                };
            }

            channel.Enabled = true;

            localSession.SaveOrUpdate(channel);
            localSession.Flush();

            this.client.JoinChannel(channelName);
        }
        
        public void PartChannel(string channelName, ISession localSession, string message)
        {
            var channel = localSession.CreateCriteria<Channel>()
                .Add(Restrictions.Eq("Name", channelName))
                .List<Channel>()
                .FirstOrDefault();

            this.client.PartChannel(channelName, message);
            
            if (channel == null)
            {
                return;
            }

            channel.Enabled = false;

            localSession.SaveOrUpdate(channel);
            localSession.Flush();
        }

        public void OnInvite(object sender, InviteEventArgs e)
        {
            const string flagRequired = Flags.BotManagement;
            
            this.flagService.GetFlagsForUser(e.User, e.Channel);
            CommandAclStatus aclStatus;

            var userIsAllowed = this.flagService.UserHasFlag(e.User, flagRequired, e.Channel);
            if (!userIsAllowed)
            {
                aclStatus = CommandAclStatus.DeniedMain;
            }
            else
            {
                this.JoinChannel(e.Channel, this.session);
                aclStatus = CommandAclStatus.Allowed;
            }

            this.accessLogService.SaveLogEntry(
                this.GetType(),
                "INVITE " + e.Channel,
                e.User,
                "INVITE",
                flagRequired,
                null,
                aclStatus);
        }

        public void OnKicked(object sender, KickedEventArgs e)
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
            this.session.Flush();
        }
    }
}