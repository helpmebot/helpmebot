﻿namespace Helpmebot.Services
{
    using System;
    using helpmebot6.Commands;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Events;

    public class ChannelManagementService : IChannelManagementService
    {
        private readonly IChannelRepository channelRepository;

        public ChannelManagementService(IChannelRepository channelRepository)
        {
            this.channelRepository = channelRepository;
        }
        
        // This needs to be ripped apart and actually dealt with properly.
        // The code from the join command should be moved here, and the join command should call in here, along with
        // the invite code too.
        public void OnInvite(object sender, InviteEventArgs e)
        {
            var legacyUser = LegacyUser.NewFromOtherUser(e.User);

            if (legacyUser == null)
            {
                throw new NullReferenceException(string.Format("Legacy user creation failed from user {0}", e.User));
            }

            // FIXME: ServiceLocator - CSH
            var commandServiceHelper = ServiceLocator.Current.GetInstance<ICommandServiceHelper>();

            new Join(legacyUser, e.Nickname, new[] {e.Channel}, commandServiceHelper).RunCommand();
        }

        public void OnKicked(object sender, KickedEventArgs e)
        {
            var channel = this.channelRepository.GetByName(e.Channel);
            channel.Enabled = false;
            this.channelRepository.Save(channel);
        }
    }
}