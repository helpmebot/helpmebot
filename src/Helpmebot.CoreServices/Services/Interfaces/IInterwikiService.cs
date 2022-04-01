namespace Helpmebot.CoreServices.Services.Interfaces
{
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public interface IInterwikiService
    {
        (int upToDate, int created, int updated, int deleted) Import(IMediaWikiApi api);
        
        void ForgetMissing();
        
        bool Accept(string prefix);

        void AddOrUpdate(string prefix, string url, out bool updated);
        
        /// <summary>
        /// Deletes a prefix
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns>true on successful deletion</returns>
        bool Delete(string prefix);

        bool Reject(string prefix);
    }
}