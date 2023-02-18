namespace Helpmebot.CoreServices.Services.Interfaces
{
    using System.Collections.Generic;
    using Helpmebot.Model;

    public interface IJoinMessageConfigurationService
    {
        IEnumerable<string> GetOverridesForChannel(string channel);
        IList<WelcomeUser> GetExceptions(string channel);
        IList<WelcomeUser> GetUsers(string channel);
        WelcomerOverride GetOverride(string channel, string welcomerFlag);

        void AddWelcomeEntry(WelcomeUser user);
        void RemoveWelcomeEntry(WelcomeUser user);
    }
}