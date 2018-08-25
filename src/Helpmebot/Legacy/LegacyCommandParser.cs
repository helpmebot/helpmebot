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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Castle.Core.Logging;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;
    using helpmebot6.Commands;
    using Helpmebot.Legacy.Transitional;
    using Stwalkerster.IrcClient.Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Stwalkerster.IrcClient.Model.Interfaces;
    using CategoryWatcher = helpmebot6.Commands.CategoryWatcher;

    public class LegacyCommandParser
    {
        private const string AllowedCommandNameChars = "0-9A-Za-z-_";
        private readonly ICommandServiceHelper commandServiceHelper;
        private readonly IRedirectionParserService redirectionParserService;
        private readonly ICategoryWatcherHelperService categoryWatcherHelperService;
        private readonly ILegacyAccessService legacyAccessService;
        private readonly string commandTrigger;
        private readonly string debugChannel;
        private readonly ILogger logger;

        public LegacyCommandParser(
            ICommandServiceHelper commandServiceHelper,
            ILogger logger,
            IRedirectionParserService redirectionParserService,
            BotConfiguration configuration,
            ICategoryWatcherHelperService categoryWatcherHelperService,
            ILegacyAccessService legacyAccessService)
        {
            this.commandServiceHelper = commandServiceHelper;
            this.redirectionParserService = redirectionParserService;
            this.categoryWatcherHelperService = categoryWatcherHelperService;
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

            /*
             * check category codes
             */
            if (this.categoryWatcherHelperService.GetValidWatcherKeys.Contains(command))
            {
                int argsLength = args.SmartLength();

                var newArgs = new string[argsLength + 1];
                int newArrayPos = 1;
                foreach (string t in args)
                {
                    if (!string.IsNullOrEmpty(t))
                    {
                        newArgs[newArrayPos] = t;
                    }

                    newArrayPos++;
                }

                newArgs[0] = command;

                var redirectionResult = this.redirectionParserService.Parse(newArgs);
                CommandResponseHandler crh =
                    new CategoryWatcher(
                        source,
                        destination,
                        redirectionResult.Message.ToArray(),
                        this.commandServiceHelper).RunCommand();
                this.HandleCommandResponseHandler(source, destination, redirectionResult.Destination, crh);
                return;
            }

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

            /*
             * Check for a learned word
             */
            {
                // FIXME: ServiceLocator - keywordservice
                var keywordService = ServiceLocator.Current.GetInstance<IKeywordService>();

                Keyword keyword = keywordService.Get(command);

                var crh = new CommandResponseHandler();
                string directedTo = string.Empty;
                if (keyword != null)
                {
                    if (this.legacyAccessService.GetLegacyUserRights(source) < LegacyUserRights.Normal)
                    {
                        this.logger.InfoFormat("Access denied for keyword retrieval for {0}", source);

                        var messageService1 = this.commandServiceHelper.MessageService;
                        crh.Respond(
                            messageService1.RetrieveMessage(Messages.OnAccessDenied, destination, null),
                            CommandResponseDestination.PrivateMessage);

                        string[] accessDeniedArguments = {source.ToString(), MethodBase.GetCurrentMethod().Name};
                        crh.Respond(
                            messageService1.RetrieveMessage("accessDeniedDebug", destination, accessDeniedArguments),
                            CommandResponseDestination.ChannelDebug);
                    }
                    else
                    {
                        string wordResponse = keyword.Response;

                        IDictionary<string, object> dict = new Dictionary<string, object>();

                        dict.Add("username", source.Username);
                        dict.Add("nickname", source.Nickname);
                        dict.Add("hostname", source.Hostname);
                        dict.Add("AccessLevel", this.legacyAccessService.GetLegacyUserRights(source));
                        dict.Add("channel", destination);

                        for (int i = 0; i < args.Length; i++)
                        {
                            dict.Add(i.ToString(CultureInfo.InvariantCulture), args[i]);
                            dict.Add(i + "*", string.Join(" ", args, i, args.Length - i));
                        }

                        wordResponse = wordResponse.FormatWith(dict);

                        if (keyword.Action)
                        {
                            crh.Respond(wordResponse.SetupForCtcp("ACTION"));
                        }
                        else
                        {
                            var redirectionResult = this.redirectionParserService.Parse(args);

                            directedTo = redirectionResult.Destination;
                            args = redirectionResult.Message.ToArray();

                            crh.Respond(wordResponse);
                        }

                        this.HandleCommandResponseHandler(source, destination, directedTo, crh);
                    }
                }
            }
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