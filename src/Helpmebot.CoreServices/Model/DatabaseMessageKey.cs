namespace Helpmebot.CoreServices.Model
{
    public class DatabaseMessageKey
    {
        public string ContextType { get; }
        public string Context { get; }
        public string MessageKey { get; }

        public DatabaseMessageKey(string contextType, string context, string messageKey)
        {
            this.ContextType = contextType;
            this.Context = context;
            this.MessageKey = messageKey;
        }

        public DatabaseMessageKey(DatabaseMessage message)
        {
            this.ContextType = message.ContextType;
            this.Context = message.Context;
            this.MessageKey = message.MessageKey;
        }

        protected bool Equals(DatabaseMessageKey other)
        {
            return this.ContextType == other.ContextType && this.Context == other.Context
                                                         && this.MessageKey == other.MessageKey;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((DatabaseMessageKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (this.ContextType != null ? this.ContextType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Context != null ? this.Context.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.MessageKey != null ? this.MessageKey.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}