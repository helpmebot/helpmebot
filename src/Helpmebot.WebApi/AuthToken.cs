namespace Helpmebot.WebApi
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class AuthToken
    {
        internal AuthToken(IUser user, string secret)
        {
            this.Secret = secret;
            this.User = user;
            this.IssueTime = DateTime.UtcNow;
            this.UsageCount = 0;
        }

        public bool CheckValidity(string token, int iv, int window)
        {
            // Short-circuit; we should never do this except for testing.
            if (token == this.Secret)
            {
                return true;
            }

            var computedToken = ComputeTokenForSecret(this.Secret, iv);

            if (token == computedToken && iv > this.UsageCount && (iv - this.UsageCount) <= window)
            {
                this.UsageCount = iv;
                return true;
            }

            return false;
        }

        public static string ComputeTokenForSecret(string secret, int iv)
        {
            string computedToken;
            using (var sha256 = SHA256.Create())
            {
                var computeHash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{secret}:{iv}"));
                computedToken = Convert.ToBase64String(computeHash);
            }

            return computedToken;
        }

        internal string Secret { get; }
            
        public int UsageCount { get; private set; }

        public DateTime IssueTime { get; }
        public IUser User { get; }
    }
}