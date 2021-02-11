namespace Helpmebot.Commands.ExtensionMethods
{
    using System.Net;
    using System.Text.RegularExpressions;
    using Helpmebot.ChannelServices.ExtensionMethods;
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
                return ircUser.GetIpAddress();
            }
            
            return null;
        }
    }
}