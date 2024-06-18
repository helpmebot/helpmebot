namespace Helpmebot.Commands.Commands.Information
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Logging;
    using CoreServices.Attributes;
    using Helpmebot.CoreServices.ExtensionMethods;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("decode")]
    [CommandFlag(Flags.Protected)]
    [HelpSummary("Decodes a hexadecimal representation of an IP address.")]
    public class DecodeCommand : CommandBase
    {
        private readonly IResponder responder;

        public DecodeCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.responder = responder;
        }

        [RequiredArguments(1)]
        [Help("<hex>")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            var validHexIp = new Regex("^[0-9A-Fa-f]{8}$");

            var input = this.Arguments[0];

            if (!validHexIp.Match(input).Success)
            {
                return this.responder.Respond("commands.command.decode.invalid", this.CommandSource);
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
            
            var key = "commands.command.decode";
            if (hostname != string.Empty)
            {
                key = "commands.command.decode.dns";
            }
            
            return this.responder.Respond(key, this.CommandSource, new object[]
            {
                input, ipAddress, hostname
            });
        }
    }
}