namespace Helpmebot.ChannelServices.Model
{
    using System.Net;
    using Stwalkerster.Bot.MediaWikiLib.Model;

    public class BlockData
    {
        public bool RegisteredUser { get; set; }
        
        public string Nickname { get; set; }
        public string Channel { get; set; }
        public BlockInformation BlockInformation { get; set; }
        public IPAddress Ip { get; set; }
        public string IpOrg { get; set; }
        public string ContribsUrl { get; set; }
        
        public override string ToString()
        {
            if (this.RegisteredUser)
            {
                return string.Format(
                    "Joined user {0} in channel {1} is blocked ({2}) because: {3} ( {4} )",
                    this.Nickname,
                    this.Channel,
                    this.BlockInformation.Target,
                    this.BlockInformation.BlockReason,
                    this.ContribsUrl);
            }
            else
            {
                var orgInfo = this.IpOrg != null ? $", org: {this.IpOrg}" : string.Empty;
                var ipInfo = $" ({this.Ip}{orgInfo})";
                
                return string.Format(
                    "Joined user {0}{4} in channel {1} is IP-blocked ({2}) because: {3} ( {5} )",
                    this.Nickname,
                    this.Channel,
                    this.BlockInformation.Target,
                    this.BlockInformation.BlockReason,
                    ipInfo,
                    this.ContribsUrl);
            }
        }
    }
}