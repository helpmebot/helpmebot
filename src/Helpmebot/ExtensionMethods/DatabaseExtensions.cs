namespace Helpmebot.ExtensionMethods
{
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;

    public static class DatabaseExtensions
    {
        public static Channel GetChannelObject(this ISession session, string commandSource)
        {
            return session.CreateCriteria<Channel>()
                .Add(Restrictions.Eq("Name", commandSource))
                .UniqueResult<Channel>();
        } 
        
        public static MediaWikiSite GetMediaWikiSiteObject(this ISession session, string commandSource)
        {
            var channelObject = session.GetChannelObject(commandSource);

            if (channelObject != null)
            {
                return channelObject.BaseWiki;
            }

            return session.CreateCriteria<MediaWikiSite>()
                .Add(Restrictions.Eq("IsDefault", 1))
                .UniqueResult<MediaWikiSite>();
        }
    }
}