namespace Helpmebot.Model
{
    using Helpmebot.Persistence;

    public class NotificationType : EntityBase
    {
        public virtual Channel Channel { get; set; }
        public virtual int Type { get; set; }
    }
}