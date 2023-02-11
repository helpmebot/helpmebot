namespace Helpmebot.CoreServices.Services.Interfaces
{
    using System.Collections.Generic;

    public interface IJoinMessageConfigurationService
    {
        IEnumerable<string> GetOverridesForChannel(string channel);
    }
}