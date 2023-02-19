namespace Helpmebot.Model
{
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Persistence;

    public class CrossChannel : EntityBase, ICommandParserEntity
    {
        public virtual string FrontendChannel { get; set; }
        public virtual string BackendChannel { get; set; }
        public virtual bool NotifyEnabled { get; set; }
        public virtual string NotifyKeyword { get; set; }
        public virtual string NotifyMessage { get; set; }
        public virtual bool ForwardEnabled { get; set; }

        public virtual string CommandKeyword => this.NotifyKeyword;

        public virtual string CommandChannel => this.FrontendChannel;
    }
}