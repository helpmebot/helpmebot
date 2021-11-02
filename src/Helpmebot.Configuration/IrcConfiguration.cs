// ReSharper disable UnusedAutoPropertyAccessor.Global - used by YAML deserialize
namespace Helpmebot.Configuration
{
    public class IrcConfiguration
    {
        public bool AuthToServices { get; set; }
        public string Hostname { get; set;}
        public string Nickname { get;  set;}
        public int Port { get;  set;}
        public string RealName { get; set; }
        public string Username { get;  set;}
        public string ServerPassword { get; set; }
        public bool Ssl { get;  set;}
        public string ClientName { get;  set;}
        public bool? RestartOnHeavyLag { get;  set;}
        public bool? ReclaimNickFromServices { get; set; }
        public string ServicesUsername { get;  set;}
        public string ServicesPassword { get;  set;}
        public int? PingInterval { get;  set;}
        public int? MissedPingLimit { get;  set;}
    }
}