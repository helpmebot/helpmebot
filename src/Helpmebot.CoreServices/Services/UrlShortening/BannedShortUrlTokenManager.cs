namespace Helpmebot.CoreServices.Services.UrlShortening
{
    using System.Collections.Generic;

    public class BannedShortUrlTokenManager
    {
        // vague list of things that should never be presented as a short url token
        // not intended to be exhaustive
        public List<string> BannedTokens => new List<string>
        {
            "arse",
            "ass",
            "bitch",
            "bugger",
            "cock",
            "crap",
            "cunt",
            "damn",
            "dick",
            "fag",
            "fuck",
            "hell",
            "nigger",
            "piss",
            "shit",
            "slut",
            "twat",
            "wank",
        };
    }
}