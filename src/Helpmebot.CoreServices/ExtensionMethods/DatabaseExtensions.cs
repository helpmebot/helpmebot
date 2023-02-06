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
    }
}