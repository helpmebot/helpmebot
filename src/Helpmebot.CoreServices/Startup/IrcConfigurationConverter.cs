namespace Helpmebot.CoreServices.Startup
{
    using Helpmebot.Configuration;
    using Stwalkerster.IrcClient.Interfaces;

    public static class IrcConfigurationConverter
    {
        public static IIrcConfiguration ToConfiguration(this IrcConfiguration config)
        {
            return new Stwalkerster.IrcClient.IrcConfiguration(
                config.Hostname,
                config.Port,
                config.AuthToServices,
                config.Nickname,
                config.Username,
                config.RealName,
                config.Ssl,
                config.ClientName,
                config.ServerPassword,
                config.ServicesUsername,
                config.ServicesPassword,
                config.ServicesCertificate,
                config.RestartOnHeavyLag.GetValueOrDefault(true),
                config.ReclaimNickFromServices.GetValueOrDefault(true),
                config.PingInterval.GetValueOrDefault(15),
                config.MissedPingLimit.GetValueOrDefault(3));
        }
    }
}