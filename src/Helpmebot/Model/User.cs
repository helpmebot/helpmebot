namespace Helpmebot.Model
{
    using System;
    using System.Collections.Generic;
    using Helpmebot.Persistence;

    public class User : EntityBase
    {
        public virtual string Mask { get; set; }

        public virtual string Account { get; set; }

        public virtual DateTime? LastModified { get; set; }

        public virtual IList<FlagGroup> AppliedFlagGroups { get; set; }

        protected bool Equals(User other)
        {
            return base.Equals(other) && string.Equals(this.Mask, other.Mask) && string.Equals(this.Account, other.Account);
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

            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Mask != null ? this.Mask.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Account != null ? this.Account.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}