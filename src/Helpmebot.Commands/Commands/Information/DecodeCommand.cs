namespace Helpmebot.Commands.Commands.Information
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("decode")]
    [CommandFlag(Flags.Protected)]
    public class DecodeCommand : CommandBase
    {
        public DecodeCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
        }

        [RequiredArguments(1)]
        [Help("<hex>", "Decodes a hexadecimal representation of an IP address.")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var validHexIp = new Regex("^[0-9A-Fa-f]{8}$");

            var input = this.Arguments[0];

            if (!validHexIp.Match(input).Success)
            {
                yield return new CommandResponse
                {
                    Message =
                        "Oops! That's not a recognised input for this command! I'm looking for 8 hexadecimal characters which represent an IPv4 address."
                }; 
            }

            var ipAddress = input.GetIpAddressFromHex();
            var hostname = string.Empty;

            try
            {
                hostname = Dns.GetHostEntry(ipAddress).HostName;
            }
            catch (SocketException)
            {
            }
            
            var resolves = "";
            if (hostname != string.Empty)
            {
                resolves = string.Format(", which resolves to '{0}'", hostname);
            }

            yield return new CommandResponse
            {
                Message = string.Format("Hex string '{0}' decodes to '{1}'{2}.", input, ipAddress, resolves)
            };
        }
    }
}