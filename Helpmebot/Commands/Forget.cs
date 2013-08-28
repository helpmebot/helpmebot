// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Forget.cs" company="Helpmebot Development Team">
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
//   Forgets a keyword
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    /// <summary>
    ///   Forgets a keyword
    /// </summary>
    internal class Forget : GenericCommand
    {
        public Forget(User source, string channel, string[] args)
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
            if (args.Length >= 1)
            {
                if (WordLearner.forget(args[0]))
                    Helpmebot6.irc.ircNotice(source.nickname, new Message().get("cmdForgetDone"));
                else
                    Helpmebot6.irc.ircNotice(source.nickname, new Message().get("cmdForgetError"));
            }
            else
            {
                string[] messageParameters = {"forget", "1", args.Length.ToString()};
                Helpmebot6.irc.ircNotice(source.nickname,
                                         new Message().get("notEnoughParameters", messageParameters));
            }
            return null;
        }
    }
}