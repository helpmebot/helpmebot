namespace Helpmebot.WebApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using Castle.Core.Logging;
    using Helpmebot.WebApi.Services.Interfaces;
    using Helpmebot.WebApi.TransportModels;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class LoginTokenService : ILoginTokenService
    {
        private const int TokenExpiryMinutes = 10;
        private const int LoginTokenBytes = 12;
        private const int AuthTokenBytes = 30;
        private const int AuthHandleBytes = LoginTokenBytes;
        private const int TokenLoggablePrefix = 3;
        private const int AuthTokenWindow = 10;
        
        private readonly ILogger logger;
        private readonly RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

        private readonly Dictionary<string, LoginToken> loginTokenStore = new Dictionary<string, LoginToken>();
        private readonly Dictionary<string, AuthToken> authTokenStore = new Dictionary<string, AuthToken>();

        public LoginTokenService(ILogger logger)
        {
            this.logger = logger;
        }
        
        public string GetLoginToken()
        {
            var data = new byte[LoginTokenBytes];
            this.random.GetBytes(data);
            var token = Convert.ToBase64String(data);

            this.PurgeExpiredTokens();
            
            this.logger.InfoFormat("Created login token {0}...", token.Substring(0, TokenLoggablePrefix));
            lock (this.loginTokenStore)
            {
                this.loginTokenStore.Add(token, new LoginToken());
            }
            
            return token;
        }

        public bool ApproveLoginToken(string token, IUser ircUser)
        {
            this.PurgeExpiredTokens();

            lock (this.loginTokenStore)
            {
                if (this.loginTokenStore.ContainsKey(token))
                {
                    this.logger.InfoFormat("Approved token {0}... for user {1}", token.Substring(0, TokenLoggablePrefix), ircUser);
                    this.loginTokenStore[token].Approve(ircUser);
                    return true;
                }
            }

            return false;
        }

        public TokenResponse GetAuthToken(string loginToken)
        {
            this.PurgeExpiredTokens();

            IUser ircUser;

            lock (this.loginTokenStore)
            {
                if (!this.loginTokenStore.ContainsKey(loginToken)
                    || !this.loginTokenStore[loginToken].Approved
                )
                {
                    throw new Exception("Invalid login token");
                }

                ircUser = this.loginTokenStore[loginToken].User;
                this.loginTokenStore.Remove(loginToken);
            }

            var handleData = new byte[AuthHandleBytes];
            this.random.GetBytes(handleData);
            var handle = Convert.ToBase64String(handleData);

            var tokenData = new byte[AuthTokenBytes];
            this.random.GetBytes(tokenData);
            var token = Convert.ToBase64String(tokenData);

            this.logger.InfoFormat("Issuing auth token handle:{0} for user {1} login {2}...", handle, ircUser, loginToken.Substring(0, TokenLoggablePrefix));

            var authToken = new AuthToken(ircUser, token);

            lock (this.authTokenStore)
            {
                this.authTokenStore.Add(handle, authToken);

                var otherSessions = this.authTokenStore.Where(x => x.Value.User.Equals(ircUser)).Select(x => x.Key).ToList();
                foreach (var session in otherSessions)
                {
                    this.authTokenStore.Remove(session);
                }
            }

            return new TokenResponse { Token = $"{handle}:{token}", IrcAccount = ircUser.Account };

        }

        public bool IsValidToken(string authToken)
        {
            if (!authToken.Contains(":"))
            {
                return false;
            }

            var tokenParts = authToken.Split(new[] { ':' }, 2);
            string handle = tokenParts[0], token = tokenParts[1];

            lock (this.authTokenStore)
            {
                if (!this.authTokenStore.ContainsKey(handle))
                {
                    return false;
                }

                // FIXME: we need an IV here too, and to test several consecutive IVs
                return this.authTokenStore[handle].CheckValidity(token, 0, AuthTokenWindow);
            }
        }

        private void PurgeExpiredTokens()
        {
            var removalCount = 0;
            lock (this.loginTokenStore)
            {
                foreach (var token in this.loginTokenStore.Keys.ToList())
                {
                    var tokenAge = DateTime.UtcNow - this.loginTokenStore[token].IssueTime;
                    if (tokenAge.TotalMinutes > TokenExpiryMinutes && !this.loginTokenStore[token].Approved)
                    {
                        this.loginTokenStore.Remove(token);
                        removalCount++;
                        continue;
                    }
                    
                    var approvalAge = DateTime.UtcNow - this.loginTokenStore[token].ApprovalTime;
                    if (approvalAge.TotalMinutes > TokenExpiryMinutes && this.loginTokenStore[token].Approved)
                    {
                        this.loginTokenStore.Remove(token);
                        removalCount++;
                        continue;
                    }
                }
            }

            this.logger.DebugFormat("Removed {0} expired tokens", removalCount);
        }

        private class LoginToken
        {
            internal LoginToken()
            {
                this.IssueTime = DateTime.UtcNow;
            }

            internal void Approve(IUser user)
            {
                this.Approved = true;
                this.ApprovalTime = DateTime.UtcNow;
                this.User = user;
            }

            public DateTime IssueTime { get; }
            public DateTime ApprovalTime { get; private set; }
            public IUser User { get; private set; }
            public bool Approved { get; private set; }
        }

        
    }
}