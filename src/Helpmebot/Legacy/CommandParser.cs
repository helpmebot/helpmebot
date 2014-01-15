// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandParser.cs" company="Helpmebot Development Team">
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
// <summary>
//   A command parser
// </summary>
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

    using Helpmebot.ExtensionMethods;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Monitoring;
    using Helpmebot.Services.Interfaces;

    using helpmebot6.Commands;

    using Microsoft.Practices.ServiceLocation;

    using CategoryWatcher = helpmebot6.Commands.CategoryWatcher;

    /// <summary>
    /// A command parser
    /// </summary>
    internal class CommandParser
    {
        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        /// The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// The allowed command name chars.
        /// </summary>
        private const string AllowedCommandNameChars = "0-9a-z-_";

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandParser"/> class. 
        /// </summary>
        public CommandParser()
        {
            // FIXME: Remove me!
            this.Log = ServiceLocator.Current.GetInstance<ILogger>();
            this.messageService = ServiceLocator.Current.GetInstance<IMessageService>();

            this.OverrideBotSilence = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [override bot silence].
        /// </summary>
        /// <value><c>true</c> if [override bot silence]; otherwise, <c>false</c>.</value>
        public bool OverrideBotSilence { get; set; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="command">The command.</param>
        /// <param name="args">The args.</param>
        public void handleCommand(LegacyUser source, string destination, string command, string[] args)
        {
            this.Log.Debug("Handling recieved message...");

            // if on ignore list, ignore!
            if (source.AccessLevel == LegacyUser.UserRights.Ignored)
            {
                return;
            }

            // flip destination over if required
            if (destination == Helpmebot6.irc.Nickname)
            {
                destination = source.Nickname;
            }

            /*
             * check category codes
             */
            if (WatcherController.Instance().IsValidKeyword(command))
            {
                int argsLength = args.SmartLength();

                string[] newArgs = new string[argsLength + 1];
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
                string directedTo = findRedirection(destination, ref newArgs);
                CommandResponseHandler crh = new CategoryWatcher(source, destination, newArgs, this.messageService).RunCommand();
                this.handleCommandResponseHandler(source, destination, directedTo, crh);
                return;
            }

            /* 
             * Check for a valid command
             * search for a class that can handle this command.
             */

            // Create a new object which holds the type of the command handler, if it exists.
            // if the command handler doesn't exist, then this won't be set to a value
            Type commandHandler =
                Type.GetType("helpmebot6.Commands." + command.Substring(0, 1).ToUpper() + command.Substring(1).ToLower());

            // check the type exists
            if (commandHandler != null)
            {
                string directedTo = findRedirection(destination, ref args);

                // create a new instance of the commandhandler.
                // cast to genericcommand (which holds all the required methods to run the command)
                // run the command.
                CommandResponseHandler response =
                    ((GenericCommand)Activator.CreateInstance(commandHandler, source, destination, args, this.messageService)).RunCommand();
                this.handleCommandResponseHandler(source, destination, directedTo, response);
                return;
            }

            /*
             * Check for a learned word
             */
            {
                // TODO: remove me
                var keywordService = ServiceLocator.Current.GetInstance<IKeywordService>();

                var keyword = keywordService.Get(command);

                var crh = new CommandResponseHandler();
                string directedTo = string.Empty;
                if (keyword != null)
                {
                    if (source.AccessLevel < LegacyUser.UserRights.Normal)
                    {
                        this.Log.InfoFormat("Access denied for keyword retrieval for {0}", source);

                        crh.respond(
                            this.messageService.RetrieveMessage(Messages.OnAccessDenied, destination, null),
                            CommandResponseDestination.PrivateMessage);

                        string[] accessDeniedArguments = { source.ToString(), MethodBase.GetCurrentMethod().Name };
                        crh.respond(
                            this.messageService.RetrieveMessage("accessDeniedDebug", destination, accessDeniedArguments),
                            CommandResponseDestination.ChannelDebug);
                    }
                    else
                    {
                        string wordResponse = keyword.Response;

                        IDictionary<string, object> dict = new Dictionary<string, object>();

                        dict.Add("username", source.Username);
                        dict.Add("nickname", source.Nickname);
                        dict.Add("hostname", source.Hostname);
                        dict.Add("AccessLevel", source.AccessLevel);
                        dict.Add("channel", destination);

                        for (int i = 0; i < args.Length; i++)
                        {
                            dict.Add(i.ToString(CultureInfo.InvariantCulture), args[i]);
                            dict.Add(i + "*", string.Join(" ", args, i, args.Length - i));
                        }

                        wordResponse = wordResponse.FormatWith(dict);

                        if (keyword.Action)
                        {
                            crh.respond(wordResponse.SetupForCtcp("ACTION"));
                        }
                        else
                        {
                            directedTo = findRedirection(destination, ref args);
                            crh.respond(wordResponse);
                        }

                        this.handleCommandResponseHandler(source, destination, directedTo, crh);
                    }
                }
            }
        }

        /// <summary>
        /// Finds the redirection.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        private static string findRedirection(string destination, ref string[] args)
        {
            var directedTo = string.Empty;
            
            foreach (string arg in args.Where(x => x.StartsWith(">")))
            {                
                directedTo = arg.Substring(1);

                GlobalFunctions.removeItemFromArray(arg, ref args);
            }
            
            return directedTo;
        }

        /// <summary>
        /// Handles the command response handler.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="directedTo">The directed to.</param>
        /// <param name="response">The response.</param>
        private void handleCommandResponseHandler(LegacyUser source, string destination, string directedTo,
                                                  CommandResponseHandler response)
        {
            if (response != null)
            {
                foreach (CommandResponse item in response.getResponses())
                {
                    string message = item.message;

                    if (directedTo != String.Empty)
                    {
                        message = directedTo + ": " + message;
                    }

                    switch (item.destination)
                    {
                        case CommandResponseDestination.Default:
                            if (this.OverrideBotSilence ||
                                LegacyConfig.singleton()["silence",destination] != "true")
                            {
                                Helpmebot6.irc.IrcPrivmsg(destination, message);
                            }
                            break;
                        case CommandResponseDestination.ChannelDebug:
                            Helpmebot6.irc.IrcPrivmsg(Helpmebot6.debugChannel, message);
                            break;
                        case CommandResponseDestination.PrivateMessage:
                            Helpmebot6.irc.IrcPrivmsg(source.Nickname, message);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///   Tests against recognised message formats
        /// </summary>
        /// <param name = "message">the message recieved</param>
        /// <param name = "overrideSilence">ref: whether this message format overrides any imposed silence</param>
        /// <returns>true if the message is in a recognised format</returns>
        /// <remarks>
        ///   Allowed formats:
        ///   !command
        ///   !helpmebot command
        ///   Helpmebot: command
        ///   Helpmebot command
        ///   Helpmebot, command
        ///   Helpmebot> command
        /// </remarks>
        public static bool isRecognisedMessage(ref string message, ref bool overrideSilence)
        {
            return parseRawLineForMessage(ref message, Helpmebot6.irc.Nickname, Helpmebot6.Trigger);
        }

        private static bool parseRawLineForMessage(ref string message, string nickname, string trigger)
        {
            Regex validCommand =
                new Regex(
                    @"^(?:" + trigger + @"(?:(?<botname>" + nickname.ToLower() +
                    @") )?(?<cmd>[" + AllowedCommandNameChars + "]+)|(?<botname>" + nickname.ToLower() +
                    @")[ ,>:](?: )?(?<cmd>[" + AllowedCommandNameChars + "]+))(?: )?(?<args>.*?)(?:\r)?$");

            Match m = validCommand.Match(message);

            if( m.Length > 0 )
            {
                message = m.Groups[ "cmd" ].Value +
                          ( m.Groups[ "args" ].Length > 0 ? " " + m.Groups[ "args" ].Value : string.Empty );
                return true;
            }

            return false;
        }
    }
}
