namespace Helpmebot.ChannelServices.Model
{
    using Stwalkerster.IrcClient.Model.Interfaces;

    internal class ExemptListEntry
    {
        public IUser User { get; set; }
        public string Exemption { get; set; }
    }
}