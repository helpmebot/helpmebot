﻿namespace Helpmebot.CoreServices.Services.Interfaces
{
    using System;
    using Helpmebot.Model;
    using Stwalkerster.IrcClient.Events;

    public interface IChannelManagementService
    {
        void OnInvite(object sender, InviteEventArgs e);
        void OnKicked(object sender, KickedEventArgs e);
        void JoinChannel(string channelName);
        void PartChannel(string channelName, string message);
        
        [Obsolete]
        Channel GetChannel(string channelName);
        void ConfigureAutolink(string channelName, bool state);
        void ConfigureSilence(string channelName, bool state);
        void ConfigureFunCommands(string channelName, bool disabled);
        bool FunCommandsDisabled(string channelName);
        bool AutoLinkEnabled(string channelName);
        bool IsSilenced(string channelName);
    }
}