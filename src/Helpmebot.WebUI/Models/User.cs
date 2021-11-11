namespace Helpmebot.WebUI.Models
{
    using System;

    public class User
    {
        public string Account { get; set; }
        public string Token { get; set; }

        protected bool Equals(User other)
        {
            return this.Account == other.Account && this.Token == other.Token;
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

            return Equals((User)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Account, this.Token);
        }
    }
}