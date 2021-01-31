namespace Helpmebot.Model
{
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Persistence;

    public class CrossChannel : EntityBase, ICommandParserEntity
    {
        public virtual Channel FrontendChannel { get; set; }
        public virtual Channel BackendChannel { get; set; }
        public virtual bool NotifyEnabled { get; set; }
        public virtual string NotifyKeyword { get; set; }
        public virtual string NotifyMessage { get; set; }
        public virtual bool ForwardEnabled { get; set; }

        public virtual string CommandKeyword
        {
            get { return this.NotifyKeyword; }
        }

        public virtual string CommandChannel
        {
            get { return this.FrontendChannel.Name; }
        }
    }
}