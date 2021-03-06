namespace Helpmebot.Model
{
    using Helpmebot.Persistence;

    public class WelcomerOverride : EntityBase
    {
        public virtual Channel Channel { get; set; }
        public virtual string ActiveFlag { get; set; }
        public virtual string Geolocation { get; set; }
        public virtual string BlockMessage { get; set; }
        public virtual string Message { get; set; }
        public virtual bool ExemptNonMatching { get; set; }
    }
}