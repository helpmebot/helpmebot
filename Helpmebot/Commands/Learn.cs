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
    using Helpmebot;

    /// <summary>
    ///   Learns a keyword
    /// </summary>
    internal class Learn : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Learn"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public Learn(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            bool action = false;
            string[] args = this.Arguments;
            if (args[0] == "@action")
            {
                action = true;
                GlobalFunctions.popFromFront(ref args);
            }

            if (args.Length >= 2)
            {
                bool hasLearntWord = WordLearner.learn(args[0], string.Join(" ", args, 1, args.Length - 1), action);
                
                string message = hasLearntWord ? new Message().get("cmdLearnDone") : new Message().get("cmdLearnError");

                Helpmebot6.irc.ircNotice(this.Source.nickname, message);
            }
            else
            {
                string[] messageParameters = { "learn", "2", args.Length.ToString() };
                Helpmebot6.irc.ircNotice(
                    this.Source.nickname,
                    new Message().get("notEnoughParameters", messageParameters));
            }

            return null;
        }
    }
}