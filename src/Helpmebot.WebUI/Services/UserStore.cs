namespace Helpmebot.WebUI.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpmebot.WebUI.Models;
    using Microsoft.AspNetCore.Identity;

    public class UserStore : IUserStore<User>
    {
        private Dictionary<string, User> sessions = new();
        
        public void LoginUser(User user)
        {
            lock (this.sessions)
            {
                if (this.sessions.ContainsKey(user.Account))
                {
                    this.sessions.Remove(user.Account);
                }
                
                this.sessions.Add(user.Account, user);
            }
        }
        
        public void LogoutUser(User user)
        {
            lock (this.sessions)
            {
                if (this.sessions.ContainsKey(user.Account))
                {
                    this.sessions.Remove(user.Account);
                }
            }
        }
        
        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            this.LoginUser(user);
            return Task.FromResult(user.Account);
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Account);
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Account);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            lock (this.sessions)
            {
                if (this.sessions.ContainsKey(userId))
                {
                    return Task.FromResult(this.sessions[userId]);
                }
            }

            return Task.FromResult<User>(null);
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            lock (this.sessions)
            {
                if (this.sessions.ContainsKey(normalizedUserName))
                {
                    return Task.FromResult(this.sessions[normalizedUserName]);
                }
            }

            return Task.FromResult<User>(null);        
        }
    }
}