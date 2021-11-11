namespace Helpmebot.WebApi.Services.Interfaces
{
    using Helpmebot.WebApi.TransportModels;
    using Stwalkerster.IrcClient.Model.Interfaces;

    /// <summary>
    /// Service to manage WebUI login sessions
    /// </summary>
    /// <para>
    /// There are two types of token exposed by this service - a login token, and an auth token.
    /// </para>
    /// <para>
    /// The login token is a temporary token issued as part of the bot's login handshake, and is only used during the
    /// login process. The auth token is a real token which authorizes access to the bot.
    /// </para>
    /// <para>
    /// 1. Get a login token from this service
    /// 2. Present the login token to the web UI.
    /// 3. The user passes the token to the bot on IRC to approve the login token
    /// 4. Web UI requests auth token from this service, passing the login token as proof
    /// </para>
    public interface ILoginTokenService
    {
        /// <summary>
        /// Get a new login token
        /// </summary>
        /// <returns></returns>
        string GetLoginToken();
        
        /// <summary>
        /// Approve a login token
        /// </summary>
        /// <param name="token">The login token to approve</param>
        /// <param name="ircUser">The approving user</param>
        /// <returns>true if the token has been successfully approved</returns>
        bool ApproveLoginToken(string token, IUser ircUser);
        
        /// <summary>
        /// Exchanges a login token for an auth token
        /// </summary>
        /// <param name="loginToken">the approved login token</param>
        /// <returns>an auth token for the user who approved the login token</returns>
        TokenResponse GetAuthToken(string loginToken);
        
        
        bool IsValidToken(string authToken);

        void InvalidateToken(string authTokenHandle);
    }
}