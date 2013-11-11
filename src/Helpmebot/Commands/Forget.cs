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
    using Helpmebot;

    /// <summary>
    ///   Forgets a keyword
    /// </summary>
    internal class Forget : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Forget"/> class.
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
        public Forget(User source, string channel, string[] args)
            : base(source, channel, args)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            if (this.Arguments.Length >= 1)
            {
                string forgottenMessage = WordLearner.forget(this.Arguments[0])
                                     ? new Message().GetMessage("cmdForgetDone")
                                     : new Message().GetMessage("cmdForgetError");

                Helpmebot6.irc.IrcNotice(this.Source.nickname, forgottenMessage);
            }
            else
            {
                string[] messageParameters = { "forget", "1", this.Arguments.Length.ToString() };
                Helpmebot6.irc.IrcNotice(
                    this.Source.nickname,
                    new Message().GetMessage("notEnoughParameters", messageParameters));
            }

            return null;
        }
    }
}