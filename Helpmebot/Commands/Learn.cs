// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Learn.cs" company="Helpmebot Development Team">
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
//   Learns a keyword
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Learns a keyword
    /// </summary>
    internal class Learn : GenericCommand
    {
        public Learn(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler ExecuteCommand(User source, string channel, string[] args)
        {
            bool action = false;
            if (args[0] == "@action")
            {
                action = true;
                GlobalFunctions.popFromFront(ref args);
            }

            if (args.Length >= 2)
            {
                Helpmebot6.irc.ircNotice( source.nickname,
                                          WordLearner.learn( args[ 0 ],
                                                             string.Join( " ",
                                                                          args,
                                                                          1,
                                                                          args.
                                                                              Length -
                                                                          1 ),
                                                             action )
                                              ? new Message().get("cmdLearnDone")
                                              : new Message().get("cmdLearnError"));
            }
            else
            {
                string[] messageParameters = {"learn", "2", args.Length.ToString()};
                Helpmebot6.irc.ircNotice(source.nickname,
                                         new Message().get("notEnoughParameters", messageParameters));
            }
            return null;
        }
    }
}