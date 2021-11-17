namespace Helpmebot.ChannelServices.Commands.Information
{
    using System.Collections.Generic;
    using System.Net;
    using Castle.Core.Logging;
    using Helpmebot.ChannelServices.ExtensionMethods;
    using Helpmebot.ChannelServices.Services.Interfaces;
    using Helpmebot.CoreServices.Model;
    using Helpmebot.CoreServices.Services.Interfaces;
    using Helpmebot.CoreServices.Services.Messages.Interfaces;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.Exceptions;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandInvocation("ipinfo")]
    [CommandInvocation("geolocate")]
    [CommandInvocation("whois")]
    [CommandFlag(Flags.Protected)]
    public class IpInfoCommand : CommandBase
    {
        private readonly IWhoisService whoisService;
        private readonly IGeolocationService geolocationService;
        private readonly IResponder responder;

        public IpInfoCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            IIrcClient client,
            IWhoisService whoisService,
            IGeolocationService geolocationService,
            IResponder responder) : base(
            commandSource,
            user,
            arguments,
            logger,
            flagService,
            configurationProvider,
            client)
        {
            this.whoisService = whoisService;
            this.geolocationService = geolocationService;
            this.responder = responder;
        }
        
        [RequiredArguments(1)]
        [Help(
            new[] {"<ip>", "<hexstring>", "<nickname>"},
            "Returns the controlling organisation and the real-world location for the provided IP address")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            try
            {
              var ip = this.GetIPAddress();
              if (ip == null)
              {
                  throw new CommandInvocationException(
                      this.responder.GetMessagePart("channelservices.command.ipinfo.no-ip", this.CommandSource));
              }

              var orgName = this.whoisService.GetOrganisationName(ip);
              var location = this.geolocationService.GetLocation(ip);

              if (orgName == null)
              {
                  return this.responder.Respond(
                      "channelservices.command.ipinfo.no-whois",
                      this.CommandSource,
                      new object[] { ip, location });
              }
              else
              {
                  return this.responder.Respond("channelservices.command.ipinfo", this.CommandSource,
                      new object[] { ip, orgName, location });
              }
            }
            catch (WebException e)
            {
                return new[] { new CommandResponse { Message = $"Exception during lookup: {e.Message}" } };
            }
        }
    }
}