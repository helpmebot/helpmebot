namespace Helpmebot.CoreServices.Services.Interfaces
{
    using System.Collections.Generic;
    using Stwalkerster.IrcClient.Events;

    public interface IChannelManagementService
    {
        void OnInvite(object sender, InviteEventArgs e);
        void OnKicked(object sender, KickedEventArgs e);
        void JoinChannel(string channelName);
        void PartChannel(string channelName, string message);

        void ConfigureAutolink(string channelName, bool state);
        void ConfigureSilence(string channelName, bool state);
        void ConfigureFunCommands(string channelName, bool disabled);
        bool FunCommandsDisabled(string channelName);
        bool AutoLinkEnabled(string channelName);
        bool IsSilenced(string channelName);
        string GetBaseWiki(string channelName);
        void SetBaseWiki(string target, string wikiWikiId);
        string GetWelcomerFlag(string channelName);
        void SetWelcomerFlag(string channelName, string welcomerFlag);

        bool IsEnabled(string channelName);
        IEnumerable<string> GetEnabledChannels();
    }
}