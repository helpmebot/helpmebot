// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyCommandParser.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Legacy
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;
    using helpmebot6.Commands;
    using Helpmebot.Legacy.Transitional;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class LegacyCommandParser
    {
        private const string AllowedCommandNameChars = "0-9A-Za-z-_";
        private readonly ICommandServiceHelper commandServiceHelper;
        private readonly IRedirectionParserService redirectionParserService;
        private readonly ILegacyAccessService legacyAccessService;
        private readonly string commandTrigger;
        private readonly string debugChannel;
        private readonly ILogger logger;

        public LegacyCommandParser(
            ICommandServiceHelper commandServiceHelper,
            ILogger logger,
            IRedirectionParserService redirectionParserService,
            BotConfiguration configuration,
            ILegacyAccessService legacyAccessService)
        {
            this.commandServiceHelper = commandServiceHelper;
            this.redirectionParserService = redirectionParserService;
            this.legacyAccessService = legacyAccessService;
            this.logger = logger;

            this.commandTrigger = configuration.CommandTrigger;
            this.debugChannel = configuration.DebugChannel;

            this.OverrideBotSilence = false;
        }

        public bool OverrideBotSilence { get; set; }

        /// <summary>
        /// Tests against recognised message formats
        /// </summary>
        /// <param name="message">
        /// the message received
        /// </param>
        /// <param name="overrideSilence">
        /// ref: whether this message format overrides any imposed silence
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// true if the message is in a recognised format
        /// </returns>
        /// <remarks>
        /// Allowed formats:
        ///     !command
        ///     !helpmebot command
        ///     Helpmebot: command
        ///     Helpmebot command
        ///     Helpmebot, command
        ///     Helpmebot&gt; command
        /// </remarks>
        public bool IsRecognisedMessage(ref string message, ref bool overrideSilence, IIrcClient client)
        {
            return ParseRawLineForMessage(ref message, client.Nickname, this.commandTrigger);
        }

        public void HandleCommand(IUser source, string destination, string command, string[] args)
        {
            this.logger.Debug("Handling received message...");

            // user is null (!)
            if (source == null)
            {
                this.logger.Debug("Ignoring message from null user.");
                return;
            }

            // if on ignore list, ignore!
            if (this.legacyAccessService.GetLegacyUserRights(source) == LegacyUserRights.Ignored)
            {
                this.logger.Debug("Ignoring message from ignored user.");
                return;
            }

            // flip destination over if required
            if (destination == this.commandServiceHelper.Client.Nickname)
            {
                destination = source.Nickname;
            }

            // category codes now handled by the new command parser 
            
            /* 
             * Check for a valid command
             * search for a class that can handle this command.
             */

            // Create a new object which holds the type of the command handler, if it exists.
            // if the command handler doesn't exist, then this won't be set to a value
            Type commandHandler =
                Type.GetType(
                    "helpmebot6.Commands." + command.Substring(0, 1).ToUpper() + command.Substring(1).ToLower());

            // check the type exists
            if (commandHandler != null)
            {
                var redirectionResult = this.redirectionParserService.Parse(args);

                // create a new instance of the commandhandler.
                // cast to genericcommand (which holds all the required methods to run the command)
                // run the command.
                var cmd = (GenericCommand)
                    Activator.CreateInstance(
                        commandHandler,
                        source,
                        destination,
                        redirectionResult.Message.ToArray(),
                        this.commandServiceHelper);
                cmd.Redirection = redirectionResult.Destination;

                CommandResponseHandler response = cmd.RunCommand();
                this.HandleCommandResponseHandler(source, destination, cmd.Redirection, response);

                return;
            }
            
            // Learned word stuff is now handed by the new command parser
        }

        private static bool ParseRawLineForMessage(ref string message, string nickname, string trigger)
        {
            var validCommand =
                new Regex(
                    @"^(?:" + trigger + @"(?:(?<botname>" + nickname.ToLower() + @") )?(?<cmd>["
                    + AllowedCommandNameChars + "]+)|(?<botname>" + nickname.ToLower() + @")[ ,>:](?: )?(?<cmd>["
                    + AllowedCommandNameChars + "]+))(?: )?(?<args>.*?)(?:\r)?$");

            Match m = validCommand.Match(message);

            if (m.Length > 0)
            {
                message = m.Groups["cmd"].Value
                          + (m.Groups["args"].Length > 0 ? " " + m.Groups["args"].Value : string.Empty);
                return true;
            }

            return false;
        }

        private void HandleCommandResponseHandler(
            IUser source,
            string destination,
            string directedTo,
            CommandResponseHandler response)
        {
            if (response != null)
            {
                foreach (CommandResponse item in response.GetResponses())
                {
                    string message = item.Message;

                    if (!string.IsNullOrEmpty(directedTo))
                    {
                        message = directedTo + ": " + message;
                    }

                    var irc1 = this.commandServiceHelper.Client;
                    switch (item.Destination)
                    {
                        case CommandResponseDestination.Default:
                            var channel = this.commandServiceHelper.ChannelRepository.GetByName(destination);
                            var silenced = channel != null && channel.Silenced;

                            if (this.OverrideBotSilence || !silenced)
                            {
                                irc1.SendMessage(destination, message);
                            }

                            break;
                        case CommandResponseDestination.ChannelDebug:
                            irc1.SendMessage(this.debugChannel, message);
                            break;
                        case CommandResponseDestination.PrivateMessage:
                            irc1.SendMessage(source.Nickname, message);
                            break;
                    }
                }
            }
        }
    }
}