#region Usings
using Twitterizer;

#endregion

namespace helpmebot6
{
    using System;

    internal class Twitter
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;

        public Twitter( string consumerKey, string consumerSecret )
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;

            _accessToken = Configuration.singleton( )[ "twitterAccessToken" ];
            _accessTokenSecret = Configuration.singleton()["twitterAccessTokenSecret"];

            if (_accessToken != "" || _accessTokenSecret != "")
                return;

            _requestToken = OAuthUtility.GetRequestToken( _consumerKey, _consumerSecret );

            Uri authorizationUri = OAuthUtility.BuildAuthorizationUri( _requestToken.Token );

            Helpmebot6.irc.ircPrivmsg( Helpmebot6.debugChannel,
                                       "Please authorise access to Twitter: " + authorizationUri );

        }

        private readonly OAuthTokenResponse _requestToken;
        private string _accessToken;
        private string _accessTokenSecret;


        public void authorise( string pinCode )
        {
            OAuthTokenResponse  accessToken = OAuthUtility.GetAccessToken( _consumerKey, _consumerSecret, _requestToken.Token, pinCode );

            Configuration.singleton( )[ "twitterAccessToken" ] = _accessToken = accessToken.Token;
            Configuration.singleton( )[ "twitterAccessTokenSecret" ] = _accessTokenSecret = accessToken.TokenSecret;
        }




        /// <summary>
        ///   Updates the authenticating user's status.  Requires the status parameter specified below.  Request must be a POST.  A status update with text identical to the authenticating user's current status will be ignored to prevent duplicates.
        /// </summary>
        /// <param name = "status">The text of your status update. URL encode as necessary. Statuses over 140 characters will cause a 403 error to be returned from the API.</param>
        /// <see cref = "http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses%C2%A0update" />
        public TwitterStatus updateStatus(string status)
        {
            status = (status.Length > 140 ? status.Substring(0, 140) : status);

            if (_accessToken == "")
                throw new TwitterizerException( "Must authorise first!" );

            OAuthTokens tokens = new OAuthTokens
                                     {
                                         AccessToken = _accessToken,
                                         AccessTokenSecret = _accessTokenSecret,
                                         ConsumerKey = _consumerKey,
                                         ConsumerSecret = _consumerSecret
                                     };

            return TwitterStatus.Update( tokens, status );
        }
    }
}