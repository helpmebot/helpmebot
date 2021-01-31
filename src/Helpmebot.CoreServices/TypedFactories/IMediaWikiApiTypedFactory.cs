namespace Helpmebot.TypedFactories
{
    using Stwalkerster.Bot.MediaWikiLib.Configuration;
    using Stwalkerster.Bot.MediaWikiLib.Services.Interfaces;

    public interface IMediaWikiApiTypedFactory
    {
        T Create<T>(IMediaWikiConfiguration mediaWikiConfiguration) where T : IMediaWikiApi;

        void Release(IMediaWikiApi mediaWikiApi);
    }
}