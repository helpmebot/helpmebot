namespace Helpmebot.CoreServices.ExtensionMethods
{
    using System;
    using Helpmebot.Model;
    using NHibernate;
    using NHibernate.Criterion;

    public static class DatabaseExtensions
    {
        [Obsolete]
        public static Channel GetChannelObject(this ISession session, string commandSource)
        {
            return session.CreateCriteria<Channel>()
                .Add(Restrictions.Eq(nameof(Channel.Name), commandSource))
                .UniqueResult<Channel>();
        } 
        
        [Obsolete("Legacy MW site database config; use CMS.GetBaseWiki() and MWAPIHelper.")]
        public static MediaWikiSite GetMediaWikiSiteObject(this ISession session, string commandSource)
        {
            var channelObject = session.GetChannelObject(commandSource);

            if (channelObject != null)
            {
                return session.CreateCriteria<MediaWikiSite>()
                    .Add(Restrictions.Eq(nameof(MediaWikiSite.WikiId), channelObject.BaseWikiId))
                    .UniqueResult<MediaWikiSite>();
            }

            return session.CreateCriteria<MediaWikiSite>()
                .Add(Restrictions.Eq(nameof(MediaWikiSite.IsDefault), true))
                .UniqueResult<MediaWikiSite>();
        }
    }
}