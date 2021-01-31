namespace Helpmebot.Commands.ExtensionMethods
{
    using System.Net;
    using System.Text.RegularExpressions;
    using NHibernate.Util;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.IrcClient.Model;
    using Helpmebot.CoreServices.ExtensionMethods;

    public static class CommandBaseExtensions
    {
        public static IPAddress GetIPAddress(this CommandBase command)
        {
            IPAddress ip;
            if (IPAddress.TryParse(command.Arguments[0], out ip))
            {
                return ip;
            }

            var match = Regex.Match(command.Arguments[0], "^[a-fA-F0-9]{8}$");
            if (match.Success)
            {
                // We've got a hex-encoded IP.
                return command.Arguments[0].GetIpAddressFromHex();
            }

            IrcUser ircUser;
            if (command.Client.UserCache.TryGetValue(command.Arguments[0], out ircUser))
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
            }
            
            return null;
        }
    }
}