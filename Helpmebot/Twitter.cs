// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Twitter.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Twitterizer wrapper class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6
{
    using System;

    using Twitterizer;

    /// <summary>
    /// Twitterizer wrapper class
    /// </summary>
    internal class Twitter
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;

        /// <summary>
        /// Initializes a new instance of the <see cref="Twitter"/> class.
        /// </summary>
        public Twitter()
        {
            _consumerKey = Configuration.singleton( )[ "twitterConsumerKey" ];
            _consumerSecret = Configuration.singleton( )[ "twitterConsumerSecret" ];

            _accessToken = Configuration.singleton( )[ "twitterAccessToken" ];
            _accessTokenSecret = Configuration.singleton( )[ "twitterAccessTokenSecret" ];



            if (Configuration.singleton()["twitterRequestToken"] != "" || ( _accessToken != "" && _accessTokenSecret != "" ))
                return;

            try
            {

                OAuthTokenResponse tkn = OAuthUtility.GetRequestToken( _consumerKey, _consumerSecret );
                Configuration.singleton( )[ "twitterRequestToken" ] = tkn.Token;


                Uri authorizationUri = OAuthUtility.BuildAuthorizationUri( tkn.Token );



                Helpmebot6.irc.ircPrivmsg( Helpmebot6.debugChannel,
                                           "Please authorise access to Twitter: " + authorizationUri );
            }
            catch (TwitterizerException ex)
            {
                GlobalFunctions.errorLog( ex );
            }
        }

        private string _accessToken;
        private string _accessTokenSecret;

        /// <summary>
        /// Authorises the specified pin code.
        /// </summary>
        /// <param name="pinCode">The pin code.</param>
        public void authorise( string pinCode )
        {
            OAuthTokenResponse accessToken = OAuthUtility.GetAccessToken(_consumerKey, _consumerSecret, Configuration.singleton()["twitterRequestToken"], pinCode);

            Configuration.singleton( )[ "twitterAccessToken" ] = _accessToken = accessToken.Token;
            Configuration.singleton( )[ "twitterAccessTokenSecret" ] = _accessTokenSecret = accessToken.TokenSecret;

            Configuration.singleton( )[ "twitterRequestToken" ] = "";
        }

        /// <summary>
        /// Deauthorises this instance.
        /// </summary>
        public void deauth( )
        {
            Configuration.singleton()["twitterAccessToken"] = "";
            Configuration.singleton()["twitterAccessTokenSecret"] = "";
        }


        /// <summary>
        ///   Updates the authenticating user's status.  Requires the status parameter specified below.  Request must be a POST.  A status update with text identical to the authenticating user's current status will be ignored to prevent duplicates.
        /// </summary>
        /// <param name = "status">The text of your status update. URL encode as necessary. Statuses over 140 characters will cause a 403 error to be returned from the API.</param>
        /// <see cref = "http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses%C2%A0update" />
        public TwitterStatus updateStatus(string status)
        {
            if (status.Length == 0) return null;
            status = (status.Length > 140 ? status.Substring(0, 140) : status);

            if (_accessToken == "")
                throw new TwitterizerException("Must authorise first!");

            OAuthTokens tokens = new OAuthTokens
                {
                    AccessToken = _accessToken,
                    AccessTokenSecret = _accessTokenSecret,
                    ConsumerKey = _consumerKey,
                    ConsumerSecret = _consumerSecret
                };
            TwitterStatus returnval;
            
            try
            {
               returnval =  TwitterStatus.Update(tokens, status);
            }
            catch (System.Net.WebException ex)
            {
                GlobalFunctions.errorLog(ex);
                returnval = null;
            }

            return returnval;
        }
    }
}