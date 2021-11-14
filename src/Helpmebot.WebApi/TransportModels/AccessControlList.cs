namespace Helpmebot.WebApi.TransportModels
{
    using System.Collections.Generic;

    public class AccessControlList
    {
        public List<UserAccessControlEntry> Users { get; set; }
    }
}