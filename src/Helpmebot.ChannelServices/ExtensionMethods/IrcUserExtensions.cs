namespace Helpmebot.ChannelServices.ExtensionMethods
{
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public static class IrcUserExtensions
    {
        public static IPAddress GetIpAddress(this IUser ircUser)
        {
            var userMatch = Regex.Match(ircUser.Username, "^[a-fA-F0-9]{8}$");
            if (userMatch.Success)
            {
                // We've got a hex-encoded IP.
                return ircUser.Username.GetIpAddressFromHex();
            }

            if (!ircUser.Hostname.Contains("/"))
            {
                // real hostname, not a cloak
                var hostAddresses = Dns.GetHostAddresses(ircUser.Hostname);
                if (hostAddresses.Length > 0)
                {
                    return hostAddresses.First() as IPAddress;
                }
            }

            return null;
        }
    }
}