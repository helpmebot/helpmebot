using System;
using Helpmebot.Persistence;

namespace Helpmebot.Model
{
    public class WelcomeLog : EntityBase
    {
        public virtual string Usermask { get; set; }
        public virtual string Channel { get; set; }
        public virtual DateTime WelcomeTimestamp { get; set; }
    }
}