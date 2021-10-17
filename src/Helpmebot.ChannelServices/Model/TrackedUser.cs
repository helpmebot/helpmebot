namespace Helpmebot.ChannelServices.Model
{
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class TrackedUser
    {
        public TrackedUser(int score, IUser user)
        {
            this.Score = score;
            this.User = user;
            this.CheckFirstMessage = false;
        }

        public int Score { get; set; }
        public IUser User { get; set; }
        
        public bool CheckFirstMessage { get; set; }
    }
}