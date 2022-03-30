namespace Helpmebot.ChannelServices.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Castle.Core;
    using Stwalkerster.IrcClient.Model;

    public interface IHelpeeManagementService : IStartable
    {
        IDictionary<IrcUser, DateTime> Helpees { get; }
        IDictionary<IrcUser, DateTime> Helpers { get; }
        string TargetChannel { get; }
    }
}