namespace Helpmebot.CoreServices.Services.Interfaces
{
    using System.Collections.Generic;

    public interface ILegacyMessageBackend
    {
        IEnumerable<string> GetRawMessages(string legacyKey);
        void RefreshResponseRepository();
    }
}