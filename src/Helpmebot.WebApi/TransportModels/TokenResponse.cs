namespace Helpmebot.WebApi.TransportModels
{
    using Stwalkerster.IrcClient.Model;

    public class TokenResponse
    {
        public string Token { get; set; }
        public string IrcAccount { get; set; }
    }
}