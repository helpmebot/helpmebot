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

namespace helpmebot6
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using helpmebot6.Commands;
    using helpmebot6.ExtensionMethods;
    using helpmebot6.Monitoring;

    using CategoryWatcher = helpmebot6.Commands.CategoryWatcher;

    /// <summary>
    /// A command parser
    /// </summary>
    internal class CommandParser
    {
        /// <summary>
        /// The allowed command name chars.
        /// </summary>
        private const string AllowedCommandNameChars = "0-9a-z-_";

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandParser"/> class. 
        /// </summary>
        public CommandParser()
        {
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
        public void handleCommand(User source, string destination, string command, string[] args)
        {
            Logger.instance().addToLog("Handling recieved message...", Logger.LogTypes.General);

            // if on ignore list, ignore!
            if (source.accessLevel == User.UserRights.Ignored)
                return;

            // flip destination over if required
            if (destination == Helpmebot6.irc.ircNickname)
                destination = source.nickname;


            /*
             * check category codes
             */
            if (WatcherController.instance().isValidKeyword(command))
            {
                int argsLength = GlobalFunctions.realArrayLength(args);

                string[] newArgs = new string[argsLength + 1];
                int newArrayPos = 1;
                for (int i = 0; i < args.Length; i++)
                {
                    if (!String.IsNullOrEmpty(args[i]))
                        newArgs[newArrayPos] = args[i];
                    newArrayPos++;
                }
                newArgs[0] = command;
                string directedTo = findRedirection(destination, ref newArgs);
                CommandResponseHandler crh = new CategoryWatcher(source, destination, newArgs).RunCommand();
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
                    ((GenericCommand)Activator.CreateInstance(commandHandler, source, destination, args)).RunCommand();
                this.handleCommandResponseHandler(source, destination, directedTo, response);
                return;
            }

            /*
             * Check for a learned word
             */
            {
                WordLearner.RemeberedWord rW = WordLearner.remember(command);
                CommandResponseHandler crh = new CommandResponseHandler();
                string wordResponse = rW.phrase;
                string directedTo = string.Empty;
                if (wordResponse != string.Empty)
                {
                    if (source.accessLevel < User.UserRights.Normal)
                    {
                        crh.respond(new Message().get("OnAccessDenied"),
                                    CommandResponseDestination.PrivateMessage);
                        string[] aDArgs = {source.ToString(), MethodBase.GetCurrentMethod().Name};
                        crh.respond(new Message().get("accessDeniedDebug", aDArgs),
                                    CommandResponseDestination.ChannelDebug);
                    }
                    else
                    {
                        IDictionary<string, object> dict = new Dictionary<string, object>();

                        dict.Add("username", source.username);
                        dict.Add("nickname", source.nickname);
                        dict.Add("hostname", source.hostname);

                        dict.Add("AccessLevel", source.accessLevel);

                        dict.Add("channel", destination);

                        for (int i = 0; i < args.Length; i++)
                        {
                            dict.Add(i.ToString(), args[i]);
                            dict.Add(i.ToString() + "*", string.Join(" ", args, i, args.Length - i));
                        }

                        wordResponse = wordResponse.FormatWith(dict);

                        if (rW.action)
                        {
                            crh.respond(IAL.wrapCTCP("ACTION", wordResponse));
                        }
                        else
                        {
                            directedTo = findRedirection(destination, ref args);
                            crh.respond(wordResponse);
                        }
                        this.handleCommandResponseHandler(source, destination, directedTo, crh);
                    }
                    return;
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
            string directedTo = "";
            foreach (string arg in args)
            {
                if (!arg.StartsWith(">")) continue;
                if (Helpmebot6.irc.isOnChannel(destination, arg.Substring(1)) != 0)
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
        private void handleCommandResponseHandler(User source, string destination, string directedTo,
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
                                Configuration.singleton()["silence",destination] != "true")
                            {
                                Helpmebot6.irc.ircPrivmsg(destination, message);
                            }
                            break;
                        case CommandResponseDestination.ChannelDebug:
                            Helpmebot6.irc.ircPrivmsg(Helpmebot6.debugChannel, message);
                            break;
                        case CommandResponseDestination.PrivateMessage:
                            Helpmebot6.irc.ircPrivmsg(source.nickname, message);
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
            return parseRawLineForMessage(ref message, Helpmebot6.irc.ircNickname, Helpmebot6.trigger);
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
                          ( m.Groups[ "args" ].Length > 0 ? " " + m.Groups[ "args" ].Value : "" );
                return true;
            }

            return false;
        }
    }
}