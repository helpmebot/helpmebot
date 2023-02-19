namespace Helpmebot.ChannelServices.Services.Interfaces
{
    using Castle.Core;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public interface ICrossChannelService : IStartable
    {
        void Configure(string frontend, string backend);
        void Deconfigure(string backend);
        void SetNotificationStatus(string backend, bool status);
        void SetNotificationMessage(string backend, string message);
        void SetNotificationKeyword(string backend, string keyword);
        bool GetNotificationStatus(string backend);
        void Notify(string frontend, string message, IIrcClient client, IUser user);
        string GetBackendChannelName(string frontend);
        string GetFrontendChannelName(string backend);
    }
}