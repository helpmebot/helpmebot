namespace Helpmebot.WebApi.TransportModels
{
    using System.Collections.Generic;

    public class UserAccessControlEntry
    {
        public string IrcMask { get; set; } 
        public string AccountName { get; set; } 
        public List<string> FlagGroups { get; set; } 
    }
}