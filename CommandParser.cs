/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/

#region Usings

using System;
using System.Reflection;
using helpmebot6.Commands;
using helpmebot6.Monitoring;
using CategoryWatcher = helpmebot6.Commands.CategoryWatcher;

#endregion

namespace helpmebot6
{
    public class CommandParser
    {
        public CommandParser()
        {
            overrideBotSilence = false;
        }

        public bool overrideBotSilence { get; set; }

        public void handleCommand(User source, string destination, string command, string[] args)
        {
            Logger.instance().addToLog(
                string.Format("Method:{0}{1}", MethodBase.GetCurrentMethod().DeclaringType.Name,
                              MethodBase.GetCurrentMethod().Name), Logger.LogTypes.DNWB);

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
                    if (!string.IsNullOrEmpty(args[i]))
                        newArgs[newArrayPos] = args[i];
                    newArrayPos++;
                }
                newArgs[0] = command;
                string directedTo = findRedirection(destination, ref newArgs);
                CommandResponseHandler crh = new CategoryWatcher().run(source, destination, newArgs);
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
                CommandResponseHandler response = ((GenericCommand) Activator.CreateInstance(commandHandler)).run(
                    source, destination, args);
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
                string directedTo = "";
                if (wordResponse != string.Empty)
                {
                    if (source.accessLevel < User.UserRights.Normal)
                    {
                        crh.respond(Configuration.singleton().getMessage("accessDenied"),
                                    CommandResponseDestination.PrivateMessage);
                        string[] aDArgs = {source.ToString(), MethodBase.GetCurrentMethod().Name};
                        crh.respond(Configuration.singleton().getMessage("accessDeniedDebug", aDArgs),
                                    CommandResponseDestination.ChannelDebug);
                    }
                    else
                    {
                        wordResponse = string.Format(wordResponse, args);
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


        private static string findRedirection(string destination, ref string[] args)
        {
            Logger.instance().addToLog(
                string.Format("Method:{0}{1}", MethodBase.GetCurrentMethod().DeclaringType.Name,
                              MethodBase.GetCurrentMethod().Name), Logger.LogTypes.DNWB);

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

        private void handleCommandResponseHandler(User source, string destination, string directedTo,
                                                  CommandResponseHandler response)
        {
            Logger.instance().addToLog(
                "Method:" + MethodBase.GetCurrentMethod().DeclaringType.Name + MethodBase.GetCurrentMethod().Name,
                Logger.LogTypes.DNWB);

            if (response != null)
            {
                foreach (CommandResponse item in response.getResponses())
                {
                    string message = item.message;

                    if (directedTo != string.Empty)
                    {
                        message = directedTo + ": " + message;
                    }

                    switch (item.destination)
                    {
                        case CommandResponseDestination.Default:
                            if (overrideBotSilence ||
                                Configuration.singleton().retrieveLocalStringOption("silence", destination) != "true")
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
    }
}